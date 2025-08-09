namespace MorningRoutine.Components.RoutineTracker

module TaskChecklist =

  open Feliz
  open MorningRoutine.Components.RoutineTracker.Types

  module Task =

    let toString = function
      | EatBreakfast -> "🍽️ Eat Breakfast"
      | GetDressed -> "👕 Get Dressed"
      | BrushedTeeth -> "🦷 Brush Teeth"

    let render completeTask (taskStatus: TaskStatus) =
      Html.label [
        prop.className "flex items-center space-x-4 cursor-pointer p-3 rounded-xl bg-white shadow-sm hover:shadow-md transition-shadow border-2 border-gray-100 hover:border-purple-200"
        prop.children [
          Html.input [
            prop.type' "checkbox"
            prop.className "h-6 w-6 text-purple-600 rounded-lg focus:ring-purple-500 focus:ring-2"
            prop.isChecked taskStatus.IsCompleted
            prop.onChange (fun (_isChecked: bool) -> completeTask taskStatus.Task)
          ]
          Html.span [
            prop.className (if taskStatus.IsCompleted then "line-through text-gray-500 text-lg" else "text-gray-700 text-lg font-medium")
            prop.text (toString taskStatus.Task)
          ]
          if taskStatus.IsCompleted then
            Html.span [
              prop.className "ml-auto text-2xl animate-pulse"
              prop.text "⭐"
            ]
        ]
      ]

    

  let render tasks completeTask =
    Html.div [
      prop.className "flex flex-col items-center w-full"
      prop.children [
        Html.div [
          prop.className "space-y-3 w-[350px]"
          prop.children (tasks |> List.map (Task.render completeTask))
        ]
      ]
    ]
