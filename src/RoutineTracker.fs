namespace MorningRoutine.Components

open System
open Feliz
open Feliz.UseElmish
open Elmish
open Browser.Dom
open Fable.Core
open Fable.Core.JS

module RoutineTracker =

  let [<Literal>] private initialTotalTime = 20.0

  type RoutineStatus =
    | NotStarted
    | Running
    | Paused
    | Stopped

  type Task =
    | EatBreakfast
    | GetDressed
    | BrushedTeeth
    | WordsPractise

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

  module State =

    let init () =
      let initialTasks = [
        { Task = EatBreakfast; IsCompleted = false }
        { Task = GetDressed; IsCompleted = false }
        { Task = BrushedTeeth; IsCompleted = false }
        { Task = WordsPractise; IsCompleted = false }
      ]

      { Status = NotStarted
        RemainingTime = TimeSpan.FromMinutes initialTotalTime
        Tasks = initialTasks }, Cmd.none

    let update msg state =
      match msg with
      | Start ->
        { state with Status = Running }, Cmd.ofMsg Tick
      | Stop ->
        { state with Status = Stopped; RemainingTime = TimeSpan.FromMinutes(initialTotalTime) }, Cmd.none
      | Pause ->
        { state with Status = Paused }, Cmd.none
      | Reset -> init ()
      | CompleteTask task ->
        let updatedTasks =
          state.Tasks
          |> List.map (fun t -> 
            if t.Task = task then { t with IsCompleted = not t.IsCompleted }
            else t)
        
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
          { newState with Status = Running }, Cmd.ofMsg Tick
        | _ -> 
          // No status change needed
          newState, Cmd.none
      | Tick ->
        match state.Status with
        | Running ->
          let newRemainingTime = state.RemainingTime.Subtract(TimeSpan.FromSeconds 1.0)
          let newState = { state with RemainingTime = newRemainingTime }

          // Check for audio prompts at 5-minute intervals
          Audio.playTimePrompt newRemainingTime

          // Check if time has run out
          if newRemainingTime <= TimeSpan.Zero then
            // Time's up - auto-pause and play completion sound
            Audio.playCompletionPrompt()
            { newState with Status = Paused; RemainingTime = TimeSpan.Zero }, Cmd.none
          else
            // Check if all tasks are completed after updating time - if so, don't schedule next tick
            let allTasksCompleted = newState.Tasks |> List.forall (fun t -> t.IsCompleted)
            if allTasksCompleted then
              // Auto-pause when all tasks are completed
              { newState with Status = Paused }, Cmd.none
            else
              // Continue ticking
              let delayedTick = async {
                do! Async.Sleep(1000)
                return Tick
              }
              newState, Cmd.OfAsync.perform id delayedTick id
        | _ ->
          state, Cmd.none

  let taskToString = function
    | EatBreakfast -> "Eat Breakfast"
    | GetDressed -> "Get Dressed"
    | BrushedTeeth -> "Brushed Teeth"
    | WordsPractise -> "Words Practise"

  let formatTime (timeSpan: TimeSpan) =
    sprintf "%02d:%02d" 
      timeSpan.Minutes 
      timeSpan.Seconds

  let formatTimeWithHours (timeSpan: TimeSpan) =
    sprintf "%02d:%02d:%02d" 
      (int timeSpan.TotalHours) 
      timeSpan.Minutes 
      timeSpan.Seconds

  let statusToString = function
    | NotStarted -> "Not Started"
    | Running -> "Running"
    | Paused -> "Paused"
    | Stopped -> "Stopped"

  [<ReactComponent>]
  let Component () =
    
    let state, dispatch = React.useElmish(State.init, State.update)

    Html.div [
      prop.className "max-w-md mx-auto p-6 bg-white rounded-lg shadow-md"
      prop.children [
        Html.h1 [
          prop.className "text-3xl font-bold text-center mb-6 text-gray-800"
          prop.text "Morning Routine Tracker"
        ]
        
        Html.div [
          prop.className "space-y-4"
          prop.children [
            // Status display
            Html.div [
              prop.className "text-center"
              prop.children [
                Html.h2 [
                  prop.className "text-xl font-semibold mb-2"
                  prop.text "Routine Status"
                ]
                Html.p [
                  prop.className "text-lg text-gray-600"
                  prop.text (statusToString state.Status)
                ]
              ]
            ]

            // Circular Timer display
            Html.div [
              prop.className "text-center"
              prop.children [
                CircularTimer.Component state.RemainingTime (TimeSpan.FromMinutes(initialTotalTime))
              ]
            ]

            // Control buttons
            Html.div [
              prop.className "flex justify-center space-x-2"
              prop.children [
                Html.button [
                  prop.className "px-4 py-2 bg-green-500 text-white rounded hover:bg-green-600 disabled:opacity-50"
                  prop.disabled (state.Status = Running || (state.Tasks |> List.forall (fun t -> t.IsCompleted)))
                  prop.onClick (fun _ -> dispatch Start)
                  prop.text "Start"
                ]
                Html.button [
                  prop.className "px-4 py-2 bg-yellow-500 text-white rounded hover:bg-yellow-600 disabled:opacity-50"
                  prop.disabled (state.Status <> Running)
                  prop.onClick (fun _ -> dispatch Pause)
                  prop.text "Pause"
                ]
                Html.button [
                  prop.className "px-4 py-2 bg-red-500 text-white rounded hover:bg-red-600 disabled:opacity-50"
                  prop.disabled (state.Status = NotStarted)
                  prop.onClick (fun _ -> dispatch Reset)
                  prop.text "Reset"
                ]
              ]
            ]

            // Tasks checklist
            Html.div [
              prop.children [
                Html.h3 [
                  prop.className "text-lg font-semibold mb-3"
                  prop.text "Morning Tasks"
                ]
                Html.div [
                  prop.className "space-y-2"
                  prop.children [
                    for taskStatus in state.Tasks do
                      Html.label [
                        prop.className "flex items-center space-x-3 cursor-pointer"
                        prop.children [
                          Html.input [
                            prop.type' "checkbox"
                            prop.className "h-4 w-4 text-blue-600 rounded focus:ring-blue-500"
                            prop.isChecked taskStatus.IsCompleted
                            prop.onChange (fun (isChecked: bool) -> dispatch (CompleteTask taskStatus.Task))
                          ]
                          Html.span [
                            prop.className (if taskStatus.IsCompleted then "line-through text-gray-500" else "text-gray-700")
                            prop.text (taskToString taskStatus.Task)
                          ]
                        ]
                      ]
                  ]
                ]
              ]
            ]
          ]
        ]
      ]
    ]
