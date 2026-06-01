namespace MorningRoutine.Components.RoutineTracker

open System
open Elmish
open MorningRoutine
open MorningRoutine.Components

module State =

  let private defaultTasks (childKey: string) = [
    { Id = sprintf "%s-breakfast" childKey; Name = "Eat Breakfast"; Icon = "🍳"; DurationMinutes = 5 }
    { Id = sprintf "%s-teeth"     childKey; Name = "Brush Teeth";   Icon = "🪥"; DurationMinutes = 2 }
    { Id = sprintf "%s-dressed"   childKey; Name = "Get Dressed";   Icon = "👕"; DurationMinutes = 3 }
    { Id = sprintf "%s-bag"       childKey; Name = "Pack Bag";      Icon = "🎒"; DurationMinutes = 3 }
  ]

  let private defaultSettings () : FamilySettings =
    let kai =
      { Id = "kai"
        Name = "Kai"
        Avatar = Rocket
        AccentColor = "#3b82f6"
        Tasks = defaultTasks "kai" }
    let brax =
      { Id = "brax"
        Name = "Braxton"
        Avatar = Dinosaur
        AccentColor = "#10b981"
        Tasks = defaultTasks "brax" }
    { Children = [ kai; brax ]
      TotalMinutes = 20
      AudioEnabled = true }

  let private freshProgress (children: Child list) : ChildProgress list =
    children
    |> List.map (fun c ->
        { ChildId = c.Id
          Tasks = c.Tasks |> List.map (fun t -> { TaskId = t.Id; Completed = false }) })

  // After settings are edited, reconcile progress so every (child, task) has an entry,
  // preserving completion state for tasks that still exist.
  let private reconcileProgress (children: Child list) (existing: ChildProgress list) : ChildProgress list =
    children
    |> List.map (fun c ->
        let existingForChild =
          existing
          |> List.tryFind (fun p -> p.ChildId = c.Id)
          |> Option.map (fun p -> p.Tasks)
          |> Option.defaultValue []
        let tasks =
          c.Tasks
          |> List.map (fun t ->
              let completed =
                existingForChild
                |> List.tryFind (fun e -> e.TaskId = t.Id)
                |> Option.map (fun e -> e.Completed)
                |> Option.defaultValue false
              { TaskId = t.Id; Completed = completed })
        { ChildId = c.Id; Tasks = tasks })

  let private totalTaskCount (settings: FamilySettings) =
    settings.Children |> List.sumBy (fun c -> c.Tasks.Length)

  let private completedTaskCount (progress: ChildProgress list) =
    progress
    |> List.sumBy (fun cp ->
        cp.Tasks |> List.filter (fun t -> t.Completed) |> List.length)

  let allTasksCompleted (state: State) =
    let total = totalTaskCount state.Settings
    total > 0 && completedTaskCount state.Progress = total

  let childProgressPercent (state: State) (childId: string) =
    state.Progress
    |> List.tryFind (fun p -> p.ChildId = childId)
    |> Option.map (fun cp ->
        let total = cp.Tasks.Length
        if total = 0 then 0.0
        else
            let doneCount = cp.Tasks |> List.filter (fun t -> t.Completed) |> List.length
            float doneCount / float total)
    |> Option.defaultValue 0.0

  let nextTaskFor (state: State) (childId: string) : Task option =
    let child = state.Settings.Children |> List.tryFind (fun c -> c.Id = childId)
    let progress = state.Progress |> List.tryFind (fun p -> p.ChildId = childId)
    match child, progress with
    | Some c, Some p ->
        c.Tasks
        |> List.tryFind (fun t ->
            p.Tasks
            |> List.tryFind (fun tc -> tc.TaskId = t.Id)
            |> Option.map (fun tc -> not tc.Completed)
            |> Option.defaultValue true)
    | _ -> None

  let private delayedTick =
    async {
      do! Async.Sleep(1000)
      return Tick
    }

  let private tickCmd = Cmd.OfAsync.perform id delayedTick id

  let private unixEpoch = DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
  let private toEpochMs (d: DateTime) = (d.ToUniversalTime() - unixEpoch).TotalMilliseconds
  let private fromEpochMs (ms: float) = unixEpoch.AddMilliseconds(ms)

  let init () =
    let today = Persistence.todayIso()
    let settings = Persistence.loadSettings() |> Option.defaultWith defaultSettings
    let storedProgress = Persistence.loadProgressFor today
    let progress =
      match storedProgress with
      | Some p -> reconcileProgress settings.Children p
      | None -> freshProgress settings.Children
    let total = TimeSpan.FromMinutes(float settings.TotalMinutes)

    // Restore the timer from the last saved state for today. A running timer
    // resumes from its absolute deadline so the countdown reflects real elapsed
    // time while the tab was closed; if the deadline already passed, it lands
    // paused at zero.
    let status, deadline, remaining, cmd =
      match Persistence.loadTimerFor today with
      | Some stored ->
          match stored.Status with
          | "Running" ->
              match stored.DeadlineEpochMs with
              | Some ms ->
                  let d = fromEpochMs ms
                  let rem = d - DateTime.UtcNow
                  if rem > TimeSpan.Zero then Running, Some d, rem, tickCmd
                  else Paused, None, TimeSpan.Zero, Cmd.none
              | None -> NotStarted, None, total, Cmd.none
          | "Paused" -> Paused, None, TimeSpan.FromSeconds stored.RemainingSeconds, Cmd.none
          | "Stopped" -> Stopped, None, total, Cmd.none
          | _ -> NotStarted, None, total, Cmd.none
      | None -> NotStarted, None, total, Cmd.none

    { Status = status
      Deadline = deadline
      RemainingTime = remaining
      TotalTime = total
      Settings = settings
      Progress = progress
      ProgressDateIso = today
      Tab = MyMorning }, cmd

  let private computeRemaining (state: State) =
    match state.Deadline with
    | Some d ->
        let remaining = d - DateTime.UtcNow
        if remaining > TimeSpan.Zero then remaining else TimeSpan.Zero
    | None -> state.RemainingTime

  let private freezeRemaining (state: State) =
    { state with RemainingTime = computeRemaining state; Deadline = None }

  let private startRunning (state: State) =
    let deadline = DateTime.UtcNow + state.RemainingTime
    { state with Status = Running; Deadline = Some deadline },
    tickCmd

  let private playPromptsForCrossings (state: State) (previous: TimeSpan) (current: TimeSpan) =
    if state.Settings.AudioEnabled then
      let crossed (mins: int) =
        let threshold = TimeSpan.FromMinutes(float mins)
        previous > threshold && current <= threshold
      if crossed 15 || crossed 10 || crossed 5 then
        Audio.playBeep()

  let private persistProgress (state: State) =
    Persistence.saveProgress state.ProgressDateIso state.Progress

  let private persistSettings (settings: FamilySettings) =
    Persistence.saveSettings settings

  let private persistTimer (state: State) =
    Persistence.saveTimer
      { Date = state.ProgressDateIso
        Status = RoutineStatus.toString state.Status
        DeadlineEpochMs = state.Deadline |> Option.map toEpochMs
        RemainingSeconds = state.RemainingTime.TotalSeconds }

  let private advanceTickState (state: State) =
    let previousRemaining = state.RemainingTime
    let newRemaining = computeRemaining state
    playPromptsForCrossings state previousRemaining newRemaining

    if newRemaining <= TimeSpan.Zero then
      if state.Settings.AudioEnabled then Audio.playCompletionPrompt()
      { state with Status = Paused; RemainingTime = TimeSpan.Zero; Deadline = None }
    elif allTasksCompleted state then
      { state with Status = Paused; RemainingTime = newRemaining; Deadline = None }
    else
      { state with RemainingTime = newRemaining }

  let private toggleTaskCompletion (childId: string) (taskId: string) (progress: ChildProgress list) =
    progress
    |> List.map (fun cp ->
        if cp.ChildId <> childId then cp
        else
            let tasks =
              cp.Tasks
              |> List.map (fun t ->
                  if t.TaskId = taskId then { t with Completed = not t.Completed }
                  else t)
            { cp with Tasks = tasks })

  let private handleCompleteTask (state: State) (childId: string) (taskId: string) =
    let wasAllDone = allTasksCompleted state
    let newProgress = toggleTaskCompletion childId taskId state.Progress
    let newState = { state with Progress = newProgress }
    persistProgress newState
    let nowAllDone = allTasksCompleted newState

    match state.Status, nowAllDone, wasAllDone with
    | Running, true, false ->
        let frozen = { freezeRemaining newState with Status = Paused }
        persistTimer frozen
        frozen, Cmd.none
    | Paused, false, true ->
        let next, cmd = startRunning newState
        persistTimer next
        next, cmd
    | _ ->
        newState, Cmd.none

  let private handleTick (state: State) =
    match state.Status with
    | Running ->
        let newState = advanceTickState state
        match newState.Status with
        | Running -> newState, tickCmd
        | _ ->
            // The countdown hit zero (or all tasks were done): persist the
            // paused result so a refresh doesn't revive a finished timer.
            persistTimer newState
            newState, Cmd.none
    | _ ->
        state, Cmd.none

  let update msg state =
    match msg with
    | Start ->
        let next, cmd = startRunning state
        persistTimer next
        next, cmd
    | Stop ->
        let total = TimeSpan.FromMinutes(float state.Settings.TotalMinutes)
        let next = { state with Status = Stopped; RemainingTime = total; Deadline = None }
        persistTimer next
        next, Cmd.none
    | Pause ->
        let frozen = { freezeRemaining state with Status = Paused }
        persistTimer frozen
        frozen, Cmd.none
    | Reset ->
        let total = TimeSpan.FromMinutes(float state.Settings.TotalMinutes)
        let progress = freshProgress state.Settings.Children
        let next =
          { state with
              Status = NotStarted
              Deadline = None
              RemainingTime = total
              TotalTime = total
              Progress = progress }
        persistProgress next
        persistTimer next
        next, Cmd.none
    | CompleteTask (childId, taskId) ->
        handleCompleteTask state childId taskId
    | Tick ->
        handleTick state
    | RefreshTime ->
        match state.Status with
        | Running -> advanceTickState state, Cmd.none
        | _ -> state, Cmd.none
    | SwitchTab tab ->
        { state with Tab = tab }, Cmd.none
    | UpdateSettings settings ->
        persistSettings settings
        let progress = reconcileProgress settings.Children state.Progress
        let total = TimeSpan.FromMinutes(float settings.TotalMinutes)
        // If routine is running, keep the existing deadline; only adjust totals while not running.
        let next =
          match state.Status with
          | Running ->
              { state with Settings = settings; Progress = progress; TotalTime = total }
          | _ ->
              { state with
                  Settings = settings
                  Progress = progress
                  TotalTime = total
                  RemainingTime = total
                  Deadline = None }
        persistProgress next
        persistTimer next
        next, Cmd.none
