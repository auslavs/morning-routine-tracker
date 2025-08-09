namespace MorningRoutine.Components

open System
open Feliz

module CircularTimer =

  let formatTime (timeSpan: TimeSpan) =
    sprintf "%02d:%02d" 
      timeSpan.Minutes 
      timeSpan.Seconds

  let getCircleColor (remainingTime: TimeSpan) (totalTime: TimeSpan) =
    let remainingMinutes = remainingTime.TotalMinutes
    if remainingMinutes <= 2.0 then "#EF4444" // red-500
    elif remainingMinutes <= 5.0 then "#F97316" // orange-500
    else "#10B981" // green-500

  let calculateProgress (remainingTime: TimeSpan) (totalTime: TimeSpan) =
    let elapsed = totalTime.Subtract(remainingTime).TotalSeconds
    let total = totalTime.TotalSeconds
    if total > 0.0 then (elapsed / total) * 100.0 else 0.0

  [<ReactComponent>]
  let Component (remainingTime: TimeSpan) (totalTime: TimeSpan) =
    let progress = calculateProgress remainingTime totalTime
    let color = getCircleColor remainingTime totalTime
    let circumference = 2.0 * System.Math.PI * 90.0 // radius = 90
    let strokeDashoffset = circumference - (progress / 100.0) * circumference

    Html.div [
      prop.className "relative w-48 h-48 mx-auto mb-4"
      prop.children [
        // SVG Circle
        Svg.svg [
          svg.className "w-full h-full transform -rotate-90"
          svg.children [
            // Background circle
            Svg.circle [
              svg.cx 96
              svg.cy 96
              svg.r 90
              svg.stroke "#E5E7EB" // gray-200
              svg.strokeWidth 8
              svg.fill "transparent"
            ]
            // Progress circle
            Svg.circle [
              svg.cx 96
              svg.cy 96
              svg.r 90
              svg.stroke color
              svg.strokeWidth 8
              svg.fill "transparent"
              svg.strokeDasharray [|int circumference; int circumference |]
              svg.strokeDashoffset strokeDashoffset

              // svg.style [
              //   style.strokeDasharray strokeDasharray
              //   style.strokeDashoffset strokeDashoffset
              //   style.transition "stroke-dashoffset 1s ease-in-out, stroke 0.3s ease"
              // ]
              svg.strokeLineCap "round"
            ]
          ]
        ]
        // Time display in center
        Html.div [
          prop.className "absolute inset-0 flex items-center justify-center"
          prop.children [
            Html.div [
              prop.className "text-center"
              prop.children [
                Html.div [
                  prop.className "text-3xl font-mono font-bold text-gray-800"
                  prop.text (formatTime remainingTime)
                ]
                Html.div [
                  prop.className "text-sm text-gray-600 mt-1"
                  prop.text "remaining"
                ]
              ]
            ]
          ]
        ]
      ]
    ]
