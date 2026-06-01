namespace MorningRoutine.Components.RoutineTracker

module Component =
  open Feliz
  open Feliz.UseElmish
  open MorningRoutine.Components.RoutineTracker

  [<ReactComponent>]
  let Render () =

    let state, dispatch = React.useElmish(State.init, State.update)

    // Recompute remaining time from the wall clock the moment the tab regains
    // visibility/focus so the user doesn't see a stale countdown.
    React.useEffect((fun () ->
      let refresh = fun (_: Browser.Types.Event) -> dispatch RefreshTime
      Browser.Dom.document.addEventListener("visibilitychange", refresh)
      Browser.Dom.window.addEventListener("focus", refresh)
      fun () ->
        Browser.Dom.document.removeEventListener("visibilitychange", refresh)
        Browser.Dom.window.removeEventListener("focus", refresh)
    ), [||])

    let body =
      match state.Tab with
      | MyMorning -> MyMorning.render state dispatch
      | Rewards ->
          Placeholder.render
            "Rewards"
            "⭐"
            "Streaks and rewards land here once we've got daily progress logging working."
      | Progress ->
          Placeholder.render
            "Progress"
            "📊"
            "A history of your family's mornings will live here."
      | Settings ->
          SettingsView.render state dispatch

    Html.div [
      prop.className "min-h-screen bg-gradient-to-b from-sky-50 to-purple-50"
      prop.children [
        Html.div [
          prop.className "max-w-3xl mx-auto p-4 md:p-6 pb-28"
          prop.children [ body ]
        ]
        BottomNav.render state.Tab dispatch
      ]
    ]
