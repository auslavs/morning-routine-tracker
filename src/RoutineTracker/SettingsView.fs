namespace MorningRoutine.Components.RoutineTracker

open System
open Feliz

module SettingsView =

  // Nine-colour palette pulled from the Morning Routine asset-pack reference.
  let private accentPalette = [
    "#2563eb"  // blue
    "#16a34a"  // green
    "#facc15"  // yellow
    "#f97316"  // orange
    "#9333ea"  // purple
    "#ec4899"  // pink
    "#06b6d4"  // sky
    "#64748b"  // gray
    "#1e293b"  // navy
  ]

  let private newId (prefix: string) =
    sprintf "%s-%d" prefix (DateTime.UtcNow.Ticks)

  let private replaceChild (settings: FamilySettings) (newChild: Child) =
    { settings with
        Children =
          settings.Children
          |> List.map (fun c -> if c.Id = newChild.Id then newChild else c) }

  let private removeChild (settings: FamilySettings) (childId: string) =
    { settings with
        Children = settings.Children |> List.filter (fun c -> c.Id <> childId) }

  let private addChild (settings: FamilySettings) =
    let palette = accentPalette
    let nextColor =
      let used = settings.Children |> List.map (fun c -> c.AccentColor) |> Set.ofList
      palette |> List.tryFind (fun c -> not (used.Contains c)) |> Option.defaultValue palette.Head
    let child =
      { Id = newId "child"
        Name = "New Kid"
        Avatar = Unicorn
        AccentColor = nextColor
        Tasks = [
          { Id = newId "task"; Name = "Eat Breakfast"; Icon = "🍳"; DurationMinutes = 5 }
        ] }
    { settings with Children = settings.Children @ [ child ] }

  let private replaceTask (child: Child) (newTask: Task) =
    { child with
        Tasks =
          child.Tasks
          |> List.map (fun t -> if t.Id = newTask.Id then newTask else t) }

  let private removeTask (child: Child) (taskId: string) =
    { child with Tasks = child.Tasks |> List.filter (fun t -> t.Id <> taskId) }

  let private addTask (child: Child) =
    let task =
      { Id = newId "task"
        Name = "New Task"
        Icon = "✨"
        DurationMinutes = 3 }
    { child with Tasks = child.Tasks @ [ task ] }

  let private taskRow (child: Child) (onChildChange: Child -> unit) (task: Task) =
    Html.div [
      prop.className "flex items-center gap-2 p-2 rounded-xl bg-gray-50"
      prop.children [
        Html.input [
          prop.className "w-14 text-center bg-white rounded-lg border border-gray-200 px-1 py-1 text-lg"
          prop.value task.Icon
          prop.onChange (fun (v: string) ->
            onChildChange (replaceTask child { task with Icon = v }))
        ]
        Html.input [
          prop.className "flex-1 bg-white rounded-lg border border-gray-200 px-2 py-1"
          prop.value task.Name
          prop.onChange (fun (v: string) ->
            onChildChange (replaceTask child { task with Name = v }))
        ]
        Html.input [
          prop.className "w-16 bg-white rounded-lg border border-gray-200 px-2 py-1 text-right"
          prop.type' "number"
          prop.min 1
          prop.max 60
          prop.value task.DurationMinutes
          prop.onChange (fun (v: string) ->
            match Int32.TryParse v with
            | true, n when n >= 1 && n <= 60 ->
                onChildChange (replaceTask child { task with DurationMinutes = n })
            | _ -> ())
        ]
        Html.span [
          prop.className "text-xs text-gray-500"
          prop.text "min"
        ]
        Html.button [
          prop.type' "button"
          prop.className "w-8 h-8 rounded-lg bg-red-50 text-red-500 hover:bg-red-100"
          prop.onClick (fun _ -> onChildChange (removeTask child task.Id))
          prop.text "✕"
        ]
      ]
    ]

  let private childCard
      (onChange: FamilySettings -> unit)
      (settings: FamilySettings)
      (child: Child) =
    let replaceWith newChild = onChange (replaceChild settings newChild)
    Html.div [
      prop.className "bg-white rounded-2xl p-4 shadow-sm space-y-3"
      prop.children [
        Html.div [
          prop.className "flex items-center gap-3"
          prop.children [
            Html.div [
              prop.className "w-12 h-12 rounded-full flex items-center justify-center text-white text-2xl shadow"
              prop.style [ style.backgroundColor child.AccentColor ]
              prop.text (AvatarKind.toEmoji child.Avatar)
            ]
            Html.input [
              prop.className "flex-1 text-lg font-semibold bg-gray-50 rounded-lg border border-gray-200 px-2 py-1"
              prop.value child.Name
              prop.onChange (fun (v: string) -> replaceWith { child with Name = v })
            ]
            Html.button [
              prop.type' "button"
              prop.className "px-3 py-1.5 rounded-lg bg-red-50 text-red-500 text-sm font-medium hover:bg-red-100"
              prop.onClick (fun _ -> onChange (removeChild settings child.Id))
              prop.text "Remove"
            ]
          ]
        ]
        Html.div [
          prop.className "grid grid-cols-2 gap-2"
          prop.children [
            Html.label [
              prop.className "text-sm text-gray-600 flex flex-col gap-1"
              prop.children [
                Html.span [ prop.text "Avatar" ]
                Html.select [
                  prop.className "bg-gray-50 rounded-lg border border-gray-200 px-2 py-1.5"
                  prop.value (AvatarKind.toLabel child.Avatar)
                  prop.onChange (fun (v: string) ->
                    let chosen =
                      AvatarKind.all
                      |> List.tryFind (fun a -> AvatarKind.toLabel a = v)
                      |> Option.defaultValue child.Avatar
                    replaceWith { child with Avatar = chosen })
                  prop.children [
                    for kind in AvatarKind.all do
                      Html.option [
                        prop.value (AvatarKind.toLabel kind)
                        prop.text (sprintf "%s %s" (AvatarKind.toEmoji kind) (AvatarKind.toLabel kind))
                      ]
                  ]
                ]
              ]
            ]
            Html.label [
              prop.className "text-sm text-gray-600 flex flex-col gap-1"
              prop.children [
                Html.span [ prop.text "Accent" ]
                Html.div [
                  prop.className "flex gap-2 items-center bg-gray-50 rounded-lg border border-gray-200 px-2 py-1.5"
                  prop.children [
                    for swatch in accentPalette do
                      Html.button [
                        prop.type' "button"
                        prop.className (
                          if swatch = child.AccentColor
                          then "w-6 h-6 rounded-full ring-2 ring-offset-2 ring-blue-500"
                          else "w-6 h-6 rounded-full"
                        )
                        prop.style [ style.backgroundColor swatch ]
                        prop.onClick (fun _ ->
                          replaceWith { child with AccentColor = swatch })
                      ]
                  ]
                ]
              ]
            ]
          ]
        ]
        Html.div [
          prop.className "space-y-2 pt-2 border-t border-gray-100"
          prop.children [
            Html.div [
              prop.className "flex items-center justify-between"
              prop.children [
                Html.div [
                  prop.className "font-semibold text-gray-700"
                  prop.text "Tasks"
                ]
                Html.button [
                  prop.type' "button"
                  prop.className "px-3 py-1 rounded-lg bg-blue-50 text-blue-600 text-sm font-medium hover:bg-blue-100"
                  prop.onClick (fun _ -> replaceWith (addTask child))
                  prop.text "+ Add task"
                ]
              ]
            ]
            for task in child.Tasks do
              taskRow child replaceWith task
          ]
        ]
      ]
    ]

  let render (state: State) (dispatch: Msg -> unit) =
    let settings = state.Settings
    let onChange newSettings = dispatch (UpdateSettings newSettings)
    Html.div [
      prop.className "space-y-4"
      prop.children [
        Html.h1 [
          prop.className "text-2xl font-extrabold text-gray-800"
          prop.text "Settings"
        ]

        Html.div [
          prop.className "bg-white rounded-2xl p-4 shadow-sm space-y-3"
          prop.children [
            Html.div [
              prop.className "flex items-center justify-between gap-3"
              prop.children [
                Html.div [
                  prop.className "flex flex-col"
                  prop.children [
                    Html.label [
                      prop.className "text-sm text-gray-600"
                      prop.text "Total morning time (minutes)"
                    ]
                    Html.input [
                      prop.className "mt-1 w-24 bg-gray-50 rounded-lg border border-gray-200 px-2 py-1 text-right"
                      prop.type' "number"
                      prop.min 5
                      prop.max 60
                      prop.value settings.TotalMinutes
                      prop.onChange (fun (v: string) ->
                        match Int32.TryParse v with
                        | true, n when n >= 5 && n <= 60 ->
                            onChange { settings with TotalMinutes = n }
                        | _ -> ())
                    ]
                  ]
                ]
                Html.label [
                  prop.className "flex items-center gap-2 cursor-pointer"
                  prop.children [
                    Html.input [
                      prop.type' "checkbox"
                      prop.className "w-5 h-5 accent-blue-500"
                      prop.isChecked settings.AudioEnabled
                      prop.onChange (fun (v: bool) ->
                        onChange { settings with AudioEnabled = v })
                    ]
                    Html.span [
                      prop.className "text-sm text-gray-700"
                      prop.text "Audio prompts"
                    ]
                  ]
                ]
              ]
            ]
          ]
        ]

        for child in settings.Children do
          childCard onChange settings child

        Html.button [
          prop.type' "button"
          prop.className "w-full py-3 rounded-2xl border-2 border-dashed border-gray-300 text-gray-500 font-medium hover:bg-gray-50"
          prop.onClick (fun _ -> onChange (addChild settings))
          prop.text "+ Add child"
        ]
      ]
    ]
