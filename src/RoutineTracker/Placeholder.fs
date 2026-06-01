namespace MorningRoutine.Components.RoutineTracker

open Feliz

module Placeholder =

  let render (title: string) (icon: string) (body: string) =
    Html.div [
      prop.className "flex flex-col items-center justify-center text-center py-12 px-6 space-y-3 bg-white rounded-2xl shadow-sm"
      prop.children [
        Html.div [
          prop.className "text-6xl"
          prop.text icon
        ]
        Html.h2 [
          prop.className "text-2xl font-bold text-gray-800"
          prop.text title
        ]
        Html.p [
          prop.className "text-gray-500 max-w-md"
          prop.text body
        ]
        Html.div [
          prop.className "mt-4 inline-block px-3 py-1 bg-blue-50 text-blue-600 text-sm font-medium rounded-full"
          prop.text "Coming soon"
        ]
      ]
    ]
