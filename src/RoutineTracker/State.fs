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
      Tasks = initialTasks }, Cmd.none

  let private delayedTick =
    async {
      do! Async.Sleep(1000)
      return Tick
    }

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
    
    // Auto-pause when all tasks completed, auto-resume when unchecking if was running
    match state.Status, allTasksCompleted, wasAllCompleted with
    | Running, true, false -> 
        // Just completed all tasks - auto pause (no tick needed)
        { newState with Status = Paused }, Cmd.none
    | Paused, false, true -> 
        // Was all completed, now unchecked something - auto resume (start ticking)
        { newState with Status = Running }, Cmd.OfAsync.perform id delayedTick id
    | _ -> 
        // No status change needed
        newState, Cmd.none

  let private handleTick state =
    match state.Status with
    | Running ->
        let newRemainingTime = state.RemainingTime.Subtract(TimeSpan.FromSeconds 1.0)
        let newState = { state with RemainingTime = newRemainingTime }

        // Check for audio prompts at 5-minute intervals
        Audio.playTimePrompt newRemainingTime

        // Check if time has run out
        if newRemainingTime <= TimeSpan.Zero then
          Audio.playCompletionPrompt()
          { newState with Status = Paused; RemainingTime = TimeSpan.Zero }, Cmd.none
        else
        // Check if all tasks are completed after updating time - if so, don't schedule next tick
        let allTasksCompleted = newState.Tasks |> List.forall (fun t -> t.IsCompleted)
        if allTasksCompleted then
          // Auto-pause when all tasks are completed
          { newState with Status = Paused }, Cmd.none
        else
          newState, Cmd.OfAsync.perform id delayedTick id
      | _ ->
          state, Cmd.none

  let update msg state =
    match msg with
    | Start ->
      { state with Status = Running }, Cmd.OfAsync.perform id delayedTick id
    | Stop ->
      { state with Status = Stopped; RemainingTime = state.TotalTime }, Cmd.none
    | Pause ->
      { state with Status = Paused }, Cmd.none
    | Reset -> init ()
    | CompleteTask task -> 
        handleCompleteTask task state
    | Tick ->
        handleTick state
    | AdjustTotalTime newTotalTime ->
        // Only allow adjustment when not running
        match state.Status with
        | NotStarted | Paused | Stopped ->
            let clampedTime = 
                if newTotalTime < TimeSpan.FromMinutes(1.0) then TimeSpan.FromMinutes(1.0)
                elif newTotalTime > TimeSpan.FromMinutes(60.0) then TimeSpan.FromMinutes(60.0)
                else newTotalTime
            { state with TotalTime = clampedTime; RemainingTime = clampedTime }, Cmd.none
        | Running ->
            state, Cmd.none
    