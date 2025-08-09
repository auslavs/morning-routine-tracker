namespace MorningRoutine.Components.RoutineTracker

module ControlPanel =

  open Feliz

  let private buttonClass =
    "w-20 h-20 text-white rounded-xl disabled:opacity-50 font-semibold shadow-md transform transition-transform hover:scale-105 flex flex-col items-center justify-center"

  let private buttonContent(icon: string) (text: string) = [
    Html.div [ prop.className "text-2xl mb-1"; prop.text icon ]
    Html.div [ prop.className "text-sm"; prop.text text ]
  ]

  let render state start pause reset =
    Html.div [
      prop.className "flex justify-center space-x-4"
      prop.children [
        Html.button [
          prop.className [ buttonClass; "bg-green-500 hover:bg-green-600"]
          prop.disabled (state.Status = Running || (state.Tasks |> List.forall (fun t -> t.IsCompleted)))
          prop.onClick start
          prop.children (buttonContent "🚀" "Start")
        ]
        Html.button [
          prop.className [ buttonClass; "bg-yellow-500 hover:bg-yellow-600" ]
          prop.disabled (state.Status <> Running)
          prop.onClick pause
          prop.children (buttonContent "⏸️" "Pause")
        ]
        Html.button [
          prop.className [ buttonClass; "bg-red-500 hover:bg-red-600" ]
          prop.disabled (state.Status = NotStarted)
          prop.onClick reset
          prop.children (buttonContent "🔄" "Reset")
        ]
      ]
    ]