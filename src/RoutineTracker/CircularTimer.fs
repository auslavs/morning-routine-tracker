namespace MorningRoutine.Components.RoutineTracker

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
  let Component (remainingTime: TimeSpan) (totalTime: TimeSpan) (onTotalTimeChange: TimeSpan -> unit) =
    let isDragging, setIsDragging = React.useState false
    let startY, setStartY = React.useState 0.0
    let startTotalMinutes, setStartTotalMinutes = React.useState 0.0
    
    let isTimerStarted = remainingTime < totalTime
    let progress = calculateProgress remainingTime totalTime
    let color = getCircleColor remainingTime totalTime
    let circumference = 2.0 * Math.PI * 90.0 // radius = 90
    let strokeDashoffset = circumference - (progress / 100.0) * circumference

    let handleStart (clientY: float) =
      setIsDragging true
      setStartY clientY
      setStartTotalMinutes totalTime.TotalMinutes

    let handleMove (clientY: float) =
      if isDragging then
        let deltaY = startY - clientY // Inverted: up = positive, down = negative
        let minutesDelta = deltaY / 10.0 // 10 pixels per minute
        let newMinutes = startTotalMinutes + minutesDelta
        let roundedMinutes = Math.Round(newMinutes) // Round to nearest whole minute
        let clampedMinutes = Math.Max(1.0, Math.Min(60.0, roundedMinutes))
        let newTotalTime = TimeSpan.FromMinutes(clampedMinutes)
        onTotalTimeChange newTotalTime

    let handleEnd () =
      setIsDragging false

    // Mouse event handlers
    let onMouseDown (e: Browser.Types.MouseEvent) =
      if not isTimerStarted then
        e.preventDefault()
        handleStart e.clientY

    // Touch event handlers  
    let onTouchStart (e: Browser.Types.TouchEvent) =
      if not isTimerStarted then
        e.preventDefault()
        if e.touches.Length > 0 then
          handleStart e.touches.[0].clientY

    let onTouchMove (e: Browser.Types.TouchEvent) =
      if not isTimerStarted && isDragging && e.touches.Length > 0 then
        e.preventDefault()
        handleMove e.touches.[0].clientY

    let onTouchEnd (e: Browser.Types.TouchEvent) =
      if not isTimerStarted then
        e.preventDefault()
        handleEnd()

    // Add global mouse move and up event listeners when dragging
    React.useEffect((fun () ->
      if isDragging then
        let handleGlobalMouseMove (e: Browser.Types.Event) =
          let mouseEvent = e :?> Browser.Types.MouseEvent
          handleMove mouseEvent.clientY
          
        let handleGlobalMouseUp (e: Browser.Types.Event) =
          handleEnd()

        Browser.Dom.document.addEventListener("mousemove", handleGlobalMouseMove)
        Browser.Dom.document.addEventListener("mouseup", handleGlobalMouseUp)
        
        Some(React.createDisposable(fun () ->
          Browser.Dom.document.removeEventListener("mousemove", handleGlobalMouseMove)
          Browser.Dom.document.removeEventListener("mouseup", handleGlobalMouseUp)
        ))
      else
        None
    ), [| box isDragging |])

    Html.div [
      prop.className (sprintf "relative w-48 h-48 mx-auto mb-4 select-none %s" 
        (if not isTimerStarted then "cursor-pointer" else ""))
      prop.style [
        style.custom("user-select", "none")
        style.custom("touch-action", "none")
      ]
      if not isTimerStarted then 
        prop.onMouseDown onMouseDown
      if not isTimerStarted then 
        prop.onTouchStart onTouchStart
      if not isTimerStarted then 
        prop.onTouchMove onTouchMove 
      if not isTimerStarted then 
        prop.onTouchEnd onTouchEnd
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
              svg.custom("transition", "stroke-dashoffset 1s ease-in-out, stroke 0.3s ease")
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
                if not isTimerStarted && not isDragging then
                  Html.div [
                    prop.className "text-xs text-blue-500 mt-2 opacity-70"
                    prop.text "Tap & drag to adjust"
                  ]
              ]
            ]
          ]
        ]
      ]
    ]
