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

  type AvatarKind =
    | Rocket
    | Dinosaur
    | Unicorn
    | Car
    | Dog
    | Bicycle

  module AvatarKind =
    let all = [ Rocket; Dinosaur; Unicorn; Car; Dog; Bicycle ]

    let toEmoji = function
      | Rocket -> "🚀"
      | Dinosaur -> "🦖"
      | Unicorn -> "🦄"
      | Car -> "🚗"
      | Dog -> "🐶"
      | Bicycle -> "🚴"

    let toLabel = function
      | Rocket -> "Rocket"
      | Dinosaur -> "Dinosaur"
      | Unicorn -> "Unicorn"
      | Car -> "Car"
      | Dog -> "Dog"
      | Bicycle -> "Bicycle"

  type Task = {
    Id: string
    Name: string
    Icon: string
    DurationMinutes: int
  }

  type Child = {
    Id: string
    Name: string
    Avatar: AvatarKind
    AccentColor: string
    Tasks: Task list
  }

  type TaskCompletion = {
    TaskId: string
    Completed: bool
  }

  type ChildProgress = {
    ChildId: string
    Tasks: TaskCompletion list
  }

  type Tab =
    | MyMorning
    | Rewards
    | Progress
    | Settings

  module Tab =
    let all = [ MyMorning; Rewards; Progress; Settings ]

    let toString = function
      | MyMorning -> "My Morning"
      | Rewards -> "Rewards"
      | Progress -> "Progress"
      | Settings -> "Settings"

    let toEmoji = function
      | MyMorning -> "🏠"
      | Rewards -> "⭐"
      | Progress -> "📊"
      | Settings -> "⚙️"

  type FamilySettings = {
    Children: Child list
    TotalMinutes: int
    AudioEnabled: bool
  }

  type State = {
    Status: RoutineStatus
    Deadline: DateTime option
    RemainingTime: TimeSpan
    TotalTime: TimeSpan
    Settings: FamilySettings
    Progress: ChildProgress list
    ProgressDateIso: string
    Tab: Tab
  }

  type Msg =
    | Start
    | Stop
    | Pause
    | Reset
    | Tick
    | RefreshTime
    | CompleteTask of childId: string * taskId: string
    | SwitchTab of Tab
    | UpdateSettings of FamilySettings
