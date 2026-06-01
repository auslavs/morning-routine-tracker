namespace MorningRoutine.Components.RoutineTracker

open Feliz

module Journey =

  type ChildJourney = {
    Child: Child
    Percent: float
    Completed: int
    Total: int
  }

  // Horizontal band along which avatars travel — the path in the
  // landscape image runs roughly from x=18% (front gate of the house) to
  // x=78% (front of the school).
  let private pathLeftPct = 18.0
  let private pathRightPct = 78.0
  let private pathWidthPct = pathRightPct - pathLeftPct

  let private positionForPercent (percent: float) =
    let clamped = max 0.0 (min 1.0 percent)
    pathLeftPct + (clamped * pathWidthPct)

  // Spread N children across vertical lanes near the bottom of the image
  // (66% .. 88% of container height) so the road/path stays visible above.
  let private laneY (index: int) (count: int) =
    if count <= 1 then 84.0
    else
      let top = 66.0
      let bottom = 88.0
      top + (float index / float (count - 1)) * (bottom - top)

  let private clamp lo hi v = max lo (min hi v)

  let private laneElements (count: int) (index: int) (j: ChildJourney) : ReactElement list =
    let yPct = laneY index count
    let xPct = positionForPercent j.Percent
    let dotCount = max 1 j.Total
    let segment = pathWidthPct / float (dotCount + 1)
    let dotXAt i = pathLeftPct + segment * float (i + 1)
    [
      // Translucent lane band so dots, names and the track stay legible
      // over the illustrated landscape underneath. -translate-y-1/2 centers
      // the band on yPct (CSS percentage margins resolve against parent
      // width, so we use a transform instead).
      Html.div [
        prop.className "absolute -translate-y-1/2 rounded-full bg-white/55 backdrop-blur-sm ring-1 ring-white/40 shadow-sm"
        prop.style [
          style.top (length.perc yPct)
          style.left (length.perc 1.0)
          style.width (length.perc 98.0)
          style.height (length.perc 22.0)
        ]
      ]

      // Track line — backdrop
      Html.div [
        prop.className "absolute h-1.5 rounded-full bg-white/70 shadow-sm"
        prop.style [
          style.top (length.perc yPct)
          style.left (length.perc pathLeftPct)
          style.width (length.perc pathWidthPct)
          style.marginTop (length.px -3)
        ]
      ]

      // Track line — filled portion (matches accent colour)
      Html.div [
        prop.className "absolute h-1.5 rounded-full shadow transition-all duration-500"
        prop.style [
          style.top (length.perc yPct)
          style.left (length.perc pathLeftPct)
          style.width (length.perc (pathWidthPct * clamp 0.0 1.0 j.Percent))
          style.marginTop (length.px -3)
          style.backgroundColor j.Child.AccentColor
        ]
      ]

      // Child name label pinned to the start of the track
      Html.div [
        prop.className "absolute font-bold text-xs md:text-sm bg-white/85 backdrop-blur rounded-full px-2 py-0.5 shadow border-2"
        prop.style [
          style.top (length.perc yPct)
          style.left (length.perc 2.0)
          style.marginTop (length.px -12)
          style.color j.Child.AccentColor
          style.borderColor j.Child.AccentColor
        ]
        prop.text j.Child.Name
      ]

      // Task dots
      for i in 0 .. dotCount - 1 do
        let filled = i < j.Completed
        Html.div [
          prop.className "absolute -translate-x-1/2 -translate-y-1/2 w-4 h-4 md:w-5 md:h-5 rounded-full border-2 border-white shadow flex items-center justify-center text-white text-[9px] md:text-[10px] font-bold"
          prop.style [
            style.top (length.perc yPct)
            style.left (length.perc (dotXAt i))
            style.backgroundColor (if filled then j.Child.AccentColor else "#e5e7eb")
          ]
          prop.text (if filled then "✓" else "")
        ]

      // Percent badge above current position
      Html.div [
        prop.className "absolute -translate-x-1/2 -translate-y-full bg-white/95 backdrop-blur px-2 py-0.5 rounded-full text-xs font-extrabold shadow ring-1 ring-black/5"
        prop.style [
          style.top (length.perc (yPct - 7.0))
          style.left (length.perc xPct)
          style.color j.Child.AccentColor
        ]
        prop.text (sprintf "%.0f%%" (j.Percent * 100.0))
      ]

      // Avatar character at progress position
      Html.div [
        prop.className "absolute -translate-x-1/2 -translate-y-1/2 text-3xl md:text-5xl drop-shadow-lg select-none transition-all duration-500"
        prop.style [
          style.top (length.perc yPct)
          style.left (length.perc xPct)
        ]
        prop.text (AvatarKind.toEmoji j.Child.Avatar)
      ]
    ]

  // `header` is rendered as an overlay in front of the landscape image,
  // pinned to the top where the sky leaves room (lanes sit near the bottom).
  let render (header: ReactElement) (journeys: ChildJourney list) =
    let count = journeys.Length
    let lanes =
      journeys
      |> List.mapi (fun i j -> laneElements count i j)
      |> List.concat
    let overlay =
      Html.div [
        prop.className "absolute top-0 left-0 right-0 z-10 p-4 md:p-6"
        prop.children [ header ]
      ]
    // The landscape image lives on its own layer so the bottom-edge fade
    // (mask gradient) softens only the artwork — the tracking lanes that sit
    // low in the scene stay fully opaque on top.
    let imageLayer =
      Html.div [
        prop.className "absolute inset-0"
        prop.style [
          style.custom("backgroundImage", "url('/img/journey/landscape.png')")
          style.custom("backgroundSize", "cover")
          style.custom("backgroundPosition", "center 90%")
          style.custom("maskImage", "linear-gradient(to bottom, black 80%, transparent 100%)")
          style.custom("WebkitMaskImage", "linear-gradient(to bottom, black 80%, transparent 100%)")
        ]
      ]
    Html.div [
      // Full-bleed: span the whole viewport width (breaking out of the
      // padded, max-w container) and pull up to cancel the page top padding,
      // so the landscape touches the top and side edges of the viewport.
      // Square corners; the bottom edge fades out via the masked image layer.
      prop.className "relative w-screen ml-[calc(50%-50vw)] -mt-4 md:-mt-6 aspect-[16/9] md:aspect-[21/9] max-h-[55vh]"
      prop.children (imageLayer :: overlay :: lanes)
    ]
