namespace MorningRoutine.Components.RoutineTracker

[<AutoOpen>]
module Types =

  open System

  type RoutineStatus =
    | NotStarted
    | Running
    | Paused
    | Stopped

  module RoutineStatus =
    let toString = function
      | NotStarted -> "Not Started"
      | Running -> "Running"
      | Paused -> "Paused"
      | Stopped -> "Stopped"

  type Task =
    | EatBreakfast
    | GetDressed
    | BrushedTeeth

  type TaskStatus = {
    Task: Task
    IsCompleted: bool
  }

  type State = {
    Status: RoutineStatus
    RemainingTime: TimeSpan
    TotalTime: TimeSpan
    Tasks: TaskStatus list
    // When Running, the wall-clock instant at which RemainingTime should reach zero.
    // Lets us recompute remaining time from the real clock so background-tab throttling
    // and device sleep don't cause the timer to drift.
    Deadline: DateTime option
  }

  type Msg =
    | Start
    | Stop
    | Pause
    | Reset
    | CompleteTask of Task
    | Tick
    | RefreshTime
    | AdjustTotalTime of TimeSpan
