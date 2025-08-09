namespace MorningRoutine.Components.RoutineTracker

module Component =
  open System
  open Feliz
  open Feliz.UseElmish
  open MorningRoutine.Components.RoutineTracker

  [<ReactComponent>]
  let Render () =
    
    let state, dispatch = React.useElmish(State.init, State.update)

    Html.div [
      prop.className "w-full md:max-w-md md:mx-auto md:p-8 p-6 bg-gradient-to-br from-blue-50 to-purple-50 md:rounded-2xl md:shadow-lg md:border-2 md:border-purple-200 min-h-screen md:min-h-0"
      prop.children [
        Html.h1 [
          prop.className "text-4xl font-bold text-center mb-6 text-purple-600"
          prop.text "My Morning"
        ]
        
        Html.div [
          prop.className "space-y-4"
          prop.children [
            // Status display
            Html.div [
              prop.className "text-center"
              prop.children [
                Html.p [
                  prop.className "text-lg text-gray-600"
                  prop.text (RoutineStatus.toString state.Status)
                ]
              ]
            ]

            // Circular Timer display
            Html.div [
              prop.className "text-center"
              prop.children [
                CircularTimer.Component state.RemainingTime state.TotalTime (dispatch << AdjustTotalTime)
              ]
            ]

            ControlPanel.render state
              (fun _ -> Start |> dispatch)
              (fun _ -> Pause |> dispatch)
              (fun _ -> Reset |> dispatch)

            TaskChecklist.render state.Tasks (dispatch << CompleteTask)
          ]
        ]
      ]
    ]
