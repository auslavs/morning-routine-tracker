namespace App.Components

open Browser.Types
open Feliz
open Feliz.UseElmish
open Elmish
open App.HeadlessUI

module Form =

  type State =
    { SelectedItem: string option
      Query: string
      List: string list }

  type Msg =
    | SetQuery of string
    | SetSelectedItem of string

  module State =

    let init () =

      let names = [
        "Emily Carter"; "James Anderson"; "Sophia Bennett"; "Michael Turner"
        "Olivia Johnson"; "William Parker"; "Isabella Morgan"
        "Benjamin Scott"; "Charlotte Hayes"; "Ethan Brooks"
      ]

      { SelectedItem = None
        Query = ""
        List = names }, Cmd.none


    let update msg state =
      match msg with
      | SetQuery query ->
        { state with Query = query }, Cmd.none
      | SetSelectedItem item ->
        { state with Query = ""; SelectedItem = Some item }, Cmd.none

  let chevronUpDown =
    Svg.svg [
      svg.className "size-5 text-gray-400"
      svg.custom("data-slot", "icon")
      svg.fill "currentColor"
      svg.viewBox (0, 0, 20, 20)
      svg.children [
        Svg.path [
          svg.custom("fill-rule", "evenodd")
          svg.custom("clip-rule", "evenodd")
          svg.d "M10.53 3.47a.75.75 0 0 0-1.06 0L6.22 6.72a.75.75 0 0 0 1.06 1.06L10 5.06l2.72 2.72a.75.75 0 1 0 1.06-1.06l-3.25-3.25Zm-4.31 9.81 3.25 3.25a.75.75 0 0 0 1.06 0l3.25-3.25a.75.75 0 1 0-1.06-1.06L10 14.94l-2.72-2.72a.75.75 0 0 0-1.06 1.06Z"
        ]
      ]
    ]

  let checkIcon =
    Svg.svg [
      svg.className "size-5"
      svg.custom("data-slot", "icon")
      svg.fill "currentColor"
      svg.viewBox (0, 0, 20, 20)
      svg.children [
        Svg.path [
          svg.custom("fill-rule", "evenodd")
          svg.custom("clip-rule", "evenodd")
          svg.d "M16.704 4.153a.75.75 0 0 1 .143 1.052l-8 10.5a.75.75 0 0 1-1.127.075l-4.5-4.5a.75.75 0 0 1 1.06-1.06l3.894 3.893 7.48-9.817a.75.75 0 0 1 1.05-.143Z"
        ]
      ]
    ]

  let checkIconContainer =
    Html.span [
      prop.className "absolute inset-y-0 right-0 items-center pr-4 text-indigo-600 group-data-[selected]:flex group-data-[focus]:text-white"
      prop.children [
        checkIcon
      ]
    ]

  let comboboxOptions (selectedItem: string option) (options: string list) =
    Combobox.Options.Component [
      Combobox.Options.Props.ClassName (
        "absolute z-10 mt-1 max-h-60 w-full overflow-auto rounded-md bg-white py-1" +
        "text-base shadow-lg ring-1 ring-black/5 focus:outline-none sm:text-sm"
      )
      Combobox.Options.Props.Children [
        for opt in options do
          Combobox.Option.Component [
            Combobox.Option.Props.ClassName "text-gray-900 cursor-default select-none relative py-2 pl-3 pr-9"
            Combobox.Option.Props.Key opt
            Combobox.Option.Props.Value opt
            Combobox.Option.Props.Children [
              Html.span [
                prop.className "block truncate group-data-[selected]:font-semibold"
                prop.text opt
              ]
              match selectedItem with
              | Some i when i = opt -> checkIconContainer
              | _ -> Html.none
            ]
          ]
      ]
    ]

  [<ReactComponent>]
  let Component () =

    let state, dispatch = React.useElmish(State.init, State.update)

    let filteredList =
      if state.Query = ""
      then state.List
      else
        state.List
        |> List.filter _.ToLower().Contains(state.Query.ToLower())

    Html.div [
      prop.className "sm:bg-gray-100 w-full min-h-screen sm:p-6 lg:p-8"
      prop.children [
        Html.div [
          prop.className "mx-auto max-w-5xl px-4 sm:py-6 lg:py-8 bg-white sm:rounded-lg sm:shadow-md"
          prop.children [
            Html.div [
              prop.className "mx-auto max-w-4xl grid"
              prop.children [
                Html.div [
                  prop.key "form"
                  prop.className "grid max-w-96"
                  prop.children [
                    Html.span [
                      prop.className "text-2xl font-bold mb-4"
                      prop.children [ Html.text "Combobox Test" ]
                    ]
                    Combobox.Component [
                      Combobox.Props.As "div"
                      Combobox.Props.Value (state.SelectedItem |> Option.defaultValue "")
                      Combobox.Props.OnChange (SetSelectedItem >> dispatch)
                      Combobox.Props.Children [
                        Html.label [
                          prop.className "block text-sm/6 font-medium text-gray-900"
                          prop.text "Assigned to"
                        ]
                        Html.div [
                          prop.className "relative mt-2"
                          prop.children [
                            Combobox.Input.Component [
                              Combobox.Input.Props.ClassName (
                                "block w-full rounded-md bg-white py-1.5 pl-3 pr-12 text-base text-gray-900" +
                                "outline outline-1 -outline-offset-1 outline-gray-300 placeholder:text-gray-400" +
                                "focus:outline focus:outline-2 focus:-outline-offset-2 focus:outline-indigo-600 sm:text-sm/6"
                              )
                              Combobox.Input.Props.DisplayValue (state.SelectedItem |> Option.defaultValue "")
                              Combobox.Input.Props.OnBlur (fun () -> SetQuery "" |> dispatch)
                              Combobox.Input.Props.OnChange (fun e ->
                                let target = e.target :?> HTMLInputElement
                                target.value |> SetQuery |> dispatch
                              )
                            ]
                            Combobox.Button.Component [
                              Combobox.Button.Props.ClassName "absolute inset-y-0 right-0 flex items-center rounded-r-md px-2 focus:outline-none"
                              Combobox.Button.Props.Children [
                                chevronUpDown
                              ]
                            ]
                            comboboxOptions state.SelectedItem filteredList
                          ]
                        ]
                      ]
                    ]
                  ]
                ]
              ]
            ]
          ]
        ]
      ]
    ]
