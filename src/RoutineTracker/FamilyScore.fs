namespace MorningRoutine.Components.RoutineTracker

open Feliz

module FamilyScore =

  // Map completion ratio [0.0, 1.0] to a star count out of 5.
  let private starCount (ratio: float) =
    let raw = ratio * 5.0
    int (System.Math.Round(raw))
    |> max 0
    |> min 5

  let stars (filled: int) =
    [ for i in 1 .. 5 ->
        Html.span [
          prop.className (
            if i <= filled then "text-yellow-400 text-2xl drop-shadow-sm"
            else "text-gray-300 text-2xl"
          )
          prop.text "★"
        ] ]

  let render (completed: int) (total: int) =
    let ratio = if total = 0 then 0.0 else float completed / float total
    let filled = starCount ratio
    Html.div [
      prop.className "bg-white/80 backdrop-blur rounded-2xl px-4 py-2 flex flex-col items-center shadow"
      prop.children [
        Html.div [
          prop.className "text-xs uppercase tracking-wider text-gray-500"
          prop.text "Family Score"
        ]
        Html.div [
          prop.className "flex items-center gap-1"
          prop.children (stars filled)
        ]
      ]
    ]
