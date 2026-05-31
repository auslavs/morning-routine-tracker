namespace MorningRoutine.Components.RoutineTracker

open System
open Elmish
open MorningRoutine.Components

module State =

  let [<Literal>] initialTotalTime = 20.0

  let init () =
    let initialTasks = [
      { Task = EatBreakfast; IsCompleted = false }
      { Task = GetDressed; IsCompleted = false }
      { Task = BrushedTeeth; IsCompleted = false }
    ]

    let totalTime = TimeSpan.FromMinutes initialTotalTime
    { Status = NotStarted
      RemainingTime = totalTime
      TotalTime = totalTime
      Tasks = initialTasks
      Deadline = None }, Cmd.none

  let private delayedTick =
    async {
      do! Async.Sleep(1000)
      return Tick
    }

  let private computeRemaining state =
    match state.Deadline with
    | Some d ->
        let remaining = d - DateTime.UtcNow
        if remaining > TimeSpan.Zero then remaining else TimeSpan.Zero
    | None -> state.RemainingTime

  let private freezeRemaining state =
    { state with RemainingTime = computeRemaining state; Deadline = None }

  let private startRunning state =
    let deadline = DateTime.UtcNow + state.RemainingTime
    { state with Status = Running; Deadline = Some deadline },
    Cmd.OfAsync.perform id delayedTick id

  // Detect crossings of the 15/10/5-minute thresholds so audio prompts still fire
  // even when ticks were delayed past the exact second mark.
  let private playPromptsForCrossings (previous: TimeSpan) (current: TimeSpan) =
    let crossed (mins: int) =
      let threshold = TimeSpan.FromMinutes(float mins)
      previous > threshold && current <= threshold
    if crossed 15 || crossed 10 || crossed 5 then
      Audio.playBeep()

  // Pure tick: advance RemainingTime from the wall clock and handle completion/end-of-time.
  // Does not schedule the next tick — callers decide.
  let private advanceTickState state =
    let previousRemaining = state.RemainingTime
    let newRemaining = computeRemaining state
    playPromptsForCrossings previousRemaining newRemaining

    if newRemaining <= TimeSpan.Zero then
      Audio.playCompletionPrompt()
      { state with Status = Paused; RemainingTime = TimeSpan.Zero; Deadline = None }
    else
      let allTasksCompleted = state.Tasks |> List.forall (fun t -> t.IsCompleted)
      if allTasksCompleted then
        { state with Status = Paused; RemainingTime = newRemaining; Deadline = None }
      else
        { state with RemainingTime = newRemaining }

  let private handleCompleteTask task state =
    let updatedTasks =
      state.Tasks |> List.map (fun t ->
        if t.Task = task
        then { t with IsCompleted = not t.IsCompleted }
        else t
      )

    let newState = { state with Tasks = updatedTasks }
    let allTasksCompleted = updatedTasks |> List.forall (fun t -> t.IsCompleted)
    let wasAllCompleted = state.Tasks |> List.forall (fun t -> t.IsCompleted)

    match state.Status, allTasksCompleted, wasAllCompleted with
    | Running, true, false ->
        // Just completed all tasks - auto pause, freezing remaining at wall-clock value
        let frozen = freezeRemaining newState
        { frozen with Status = Paused }, Cmd.none
    | Paused, false, true ->
        // Was all completed, now unchecked something - auto resume from frozen remaining
        startRunning newState
    | _ ->
        newState, Cmd.none

  let private handleTick state =
    match state.Status with
    | Running ->
        let newState = advanceTickState state
        match newState.Status with
        | Running -> newState, Cmd.OfAsync.perform id delayedTick id
        | _ -> newState, Cmd.none
    | _ ->
        state, Cmd.none

  let update msg state =
    match msg with
    | Start ->
      startRunning state
    | Stop ->
      { state with Status = Stopped; RemainingTime = state.TotalTime; Deadline = None }, Cmd.none
    | Pause ->
      let frozen = freezeRemaining state
      { frozen with Status = Paused }, Cmd.none
    | Reset -> init ()
    | CompleteTask task ->
        handleCompleteTask task state
    | Tick ->
        handleTick state
    | RefreshTime ->
        // Fired on tab visibility/focus changes so the display catches up immediately
        // without waiting for the next scheduled tick. Does not schedule a new tick.
        match state.Status with
        | Running -> advanceTickState state, Cmd.none
        | _ -> state, Cmd.none
    | AdjustTotalTime newTotalTime ->
        // Only allow adjustment when not running
        match state.Status with
        | NotStarted | Paused | Stopped ->
            let clampedTime =
                if newTotalTime < TimeSpan.FromMinutes(1.0) then TimeSpan.FromMinutes(1.0)
                elif newTotalTime > TimeSpan.FromMinutes(60.0) then TimeSpan.FromMinutes(60.0)
                else newTotalTime
            { state with TotalTime = clampedTime; RemainingTime = clampedTime; Deadline = None }, Cmd.none
        | Running ->
            state, Cmd.none
