namespace App

module Main =

  open Feliz
  open App.Components
  open Browser.Dom

  let root = ReactDOM.createRoot(document.getElementById "feliz-app")
  root.render(Form.Component())