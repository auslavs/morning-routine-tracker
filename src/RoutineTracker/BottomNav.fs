namespace MorningRoutine.Components.RoutineTracker

open Feliz

module BottomNav =

  let private tabButton (active: Tab) (dispatch: Msg -> unit) (tab: Tab) =
    let isActive = active = tab
    let textClass =
      if isActive then "text-blue-600" else "text-gray-400"
    Html.button [
      prop.type' "button"
      prop.onClick (fun _ -> dispatch (SwitchTab tab))
      prop.className (sprintf "flex-1 flex flex-col items-center gap-1 py-2 %s transition" textClass)
      prop.children [
        Html.span [
          prop.className "text-xl"
          prop.text (Tab.toEmoji tab)
        ]
        Html.span [
          prop.className "text-xs font-medium"
          prop.text (Tab.toString tab)
        ]
      ]
    ]

  let render (active: Tab) (dispatch: Msg -> unit) =
    Html.nav [
      prop.className "fixed bottom-0 inset-x-0 bg-white border-t border-gray-200 shadow-lg pb-[env(safe-area-inset-bottom)] z-10"
      prop.children [
        Html.div [
          prop.className "max-w-3xl mx-auto flex"
          prop.children (Tab.all |> List.map (tabButton active dispatch))
        ]
      ]
    ]
