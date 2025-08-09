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
    Tasks: TaskStatus list
  }

  type Msg =
    | Start
    | Stop
    | Pause
    | Reset
    | CompleteTask of Task
    | Tick
