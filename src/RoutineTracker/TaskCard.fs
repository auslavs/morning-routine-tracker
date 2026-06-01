namespace MorningRoutine.Components.RoutineTracker

open Feliz

module TaskCard =

  type Status =
    | Completed
    | InProgress
    | UpNext

  let private statusBadge (accent: string) =
    function
    | Completed ->
        Html.div [
          prop.className "w-7 h-7 rounded-full flex items-center justify-center text-white shadow"
          prop.style [ style.backgroundColor accent ]
          prop.children [
            Html.span [ prop.className "text-sm font-bold"; prop.text "✓" ]
          ]
        ]
    | InProgress ->
        Html.div [
          prop.className "px-2 py-0.5 rounded-full text-xs font-semibold text-white shadow"
          prop.style [ style.backgroundColor accent ]
          prop.text "In progress"
        ]
    | UpNext ->
        Html.div [
          prop.className "w-7 h-7 rounded-full border-2 border-gray-300 flex items-center justify-center text-gray-400"
          prop.children [ Html.span [ prop.text "🔒" ] ]
        ]

  let render
      (accent: string)
      (status: Status)
      (task: Task)
      (onToggle: unit -> unit) =
    let bgClass =
      match status with
      | Completed -> "bg-green-50 border-green-200"
      | InProgress -> "bg-white border-blue-200 ring-2 ring-blue-100"
      | UpNext -> "bg-gray-50 border-gray-200 opacity-70"
    Html.button [
      prop.type' "button"
      prop.onClick (fun _ -> onToggle())
      prop.className (sprintf "w-full flex items-center gap-3 p-3 rounded-2xl border-2 transition shadow-sm hover:shadow text-left %s" bgClass)
      prop.children [
        Html.div [
          prop.className "w-12 h-12 rounded-xl bg-white shadow-sm flex items-center justify-center text-2xl"
          prop.text task.Icon
        ]
        Html.div [
          prop.className "flex-1"
          prop.children [
            Html.div [
              prop.className "font-semibold text-gray-800"
              prop.text task.Name
            ]
            Html.div [
              prop.className "text-xs text-gray-500 flex items-center gap-1"
              prop.children [
                Html.span [ prop.text "⏱" ]
                Html.span [ prop.text (sprintf "%d min" task.DurationMinutes) ]
              ]
            ]
          ]
        ]
        statusBadge accent status
      ]
    ]
