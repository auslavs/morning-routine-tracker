namespace MorningRoutine.Components.RoutineTracker

open Feliz

module Journey =

  type ChildJourney = {
    Child: Child
    Percent: float
    Completed: int
    Total: int
  }

  let private homeIcon =
    Html.div [
      prop.className "text-3xl md:text-4xl"
      prop.text "🏠"
    ]

  let private schoolIcon =
    Html.div [
      prop.className "text-3xl md:text-4xl"
      prop.text "🏫"
    ]

  // Renders a single horizontal track with the child's avatar positioned at percent.
  let track (j: ChildJourney) =
    let clamped = max 0.0 (min 1.0 j.Percent)
    let percentStr = sprintf "%.0f%%" (clamped * 100.0)
    Html.div [
      prop.className "flex flex-col gap-1"
      prop.children [
        Html.div [
          prop.className "flex items-center justify-between text-sm font-semibold"
          prop.children [
            Html.span [
              prop.style [ style.color j.Child.AccentColor ]
              prop.text j.Child.Name
            ]
            Html.span [
              prop.className "text-gray-600"
              prop.text percentStr
            ]
          ]
        ]
        Html.div [
          prop.className "relative w-full h-12 flex items-center"
          prop.children [
            homeIcon
            Html.div [
              prop.className "flex-1 relative mx-2"
              prop.children [
                Html.div [
                  prop.className "absolute inset-y-1/2 left-0 right-0 h-1 -mt-0.5 rounded-full bg-gray-200"
                ]
                Html.div [
                  prop.className "absolute inset-y-1/2 left-0 h-1 -mt-0.5 rounded-full transition-all duration-500"
                  prop.style [
                    style.width (length.perc (clamped * 100.0))
                    style.backgroundColor j.Child.AccentColor
                  ]
                ]
                Html.div [
                  prop.className "absolute top-1/2 -translate-x-1/2 -translate-y-1/2 text-2xl md:text-3xl transition-all duration-500 select-none"
                  prop.style [
                    style.left (length.perc (clamped * 100.0))
                  ]
                  prop.text (AvatarKind.toEmoji j.Child.Avatar)
                ]
              ]
            ]
            schoolIcon
          ]
        ]
      ]
    ]

  // Stacked per-child tracks (works for mobile + as a fallback layout).
  let render (journeys: ChildJourney list) =
    Html.div [
      prop.className "bg-gradient-to-b from-sky-100 to-emerald-50 rounded-2xl p-4 md:p-6 shadow-sm space-y-4"
      prop.children (journeys |> List.map track)
    ]
