namespace MorningRoutine

module Main =

  open Feliz
  open MorningRoutine.Components
  open Browser.Dom

  let root = ReactDOM.createRoot(document.getElementById "morning-routine-app")
  root.render(RoutineTracker.Component.Render())