namespace MorningRoutine.Components.RoutineTracker

open System
open Feliz

module MyMorning =

  let private formatRemaining (ts: TimeSpan) =
    if ts.TotalSeconds <= 0.0 then "00:00"
    else sprintf "%02d:%02d" (int ts.TotalMinutes) ts.Seconds

  let private timerBar (state: State) =
    let total = state.TotalTime.TotalSeconds
    let remaining = state.RemainingTime.TotalSeconds
    let elapsedRatio =
      if total <= 0.0 then 0.0
      else max 0.0 (min 1.0 ((total - remaining) / total))
    let completed =
      state.Progress
      |> List.sumBy (fun cp ->
          cp.Tasks |> List.filter (fun t -> t.Completed) |> List.length)
    let totalTasks =
      state.Settings.Children |> List.sumBy (fun c -> c.Tasks.Length)
    Html.div [
      prop.className "bg-white rounded-2xl p-4 shadow-sm flex items-center gap-4"
      prop.children [
        Html.div [
          prop.className "text-3xl"
          prop.text "⏰"
        ]
        Html.div [
          prop.className "flex-1"
          prop.children [
            Html.div [
              prop.className "flex items-baseline justify-between"
              prop.children [
                Html.div [
                  prop.className "text-xl font-bold text-gray-800"
                  prop.text (formatRemaining state.RemainingTime)
                ]
                Html.div [
                  prop.className "text-sm text-gray-500"
                  prop.text (sprintf "%d / %d tasks" completed totalTasks)
                ]
              ]
            ]
            Html.div [
              prop.className "mt-1 h-2 rounded-full bg-gray-200 overflow-hidden"
              prop.children [
                Html.div [
                  prop.className "h-full bg-blue-500 transition-all duration-500"
                  prop.style [ style.width (length.perc (elapsedRatio * 100.0)) ]
                ]
              ]
            ]
          ]
        ]
      ]
    ]

  let private startStopBar (state: State) (dispatch: Msg -> unit) =
    let primaryLabel, primaryIcon, primaryMsg, primaryClasses =
      match state.Status with
      | NotStarted | Stopped -> "Start Morning", "🚀", Start, "bg-blue-500 hover:bg-blue-600"
      | Running -> "Pause", "⏸️", Pause, "bg-yellow-500 hover:bg-yellow-600"
      | Paused -> "Resume", "▶️", Start, "bg-blue-500 hover:bg-blue-600"
    let allDone = State.allTasksCompleted state
    Html.div [
      prop.className "flex gap-2"
      prop.children [
        Html.button [
          prop.type' "button"
          prop.disabled allDone
          prop.onClick (fun _ -> dispatch primaryMsg)
          prop.className (sprintf "flex-1 text-white text-lg font-semibold py-3 rounded-2xl shadow disabled:opacity-50 transition %s" primaryClasses)
          prop.children [
            Html.span [ prop.className "mr-2"; prop.text primaryIcon ]
            Html.span [ prop.text primaryLabel ]
          ]
        ]
        Html.button [
          prop.type' "button"
          prop.disabled (state.Status = NotStarted)
          prop.onClick (fun _ -> dispatch Reset)
          prop.className "px-4 bg-gray-100 hover:bg-gray-200 text-gray-700 rounded-2xl shadow-sm disabled:opacity-40"
          prop.children [ Html.text "🔄" ]
        ]
      ]
    ]

  let private childTaskList (state: State) (dispatch: Msg -> unit) (child: Child) =
    let progress =
      state.Progress
      |> List.tryFind (fun p -> p.ChildId = child.Id)
    let isCompleted (taskId: string) =
      progress
      |> Option.bind (fun p -> p.Tasks |> List.tryFind (fun t -> t.TaskId = taskId))
      |> Option.map (fun tc -> tc.Completed)
      |> Option.defaultValue false
    let nextTaskId =
      State.nextTaskFor state child.Id |> Option.map (fun t -> t.Id)
    let statusFor (taskId: string) =
      if isCompleted taskId then TaskCard.Completed
      elif nextTaskId = Some taskId then TaskCard.InProgress
      else TaskCard.UpNext
    Html.div [
      prop.className "bg-white rounded-2xl p-4 shadow-sm space-y-2"
      prop.children [
        Html.div [
          prop.className "flex items-center gap-3 mb-2"
          prop.children [
            Html.div [
              prop.className "w-10 h-10 rounded-full flex items-center justify-center text-white text-lg shadow"
              prop.style [ style.backgroundColor child.AccentColor ]
              prop.text (AvatarKind.toEmoji child.Avatar)
            ]
            Html.div [
              prop.className "font-bold text-gray-800"
              prop.text (sprintf "%s's tasks" child.Name)
            ]
          ]
        ]
        for task in child.Tasks do
          TaskCard.render child.AccentColor (statusFor task.Id) task (fun () ->
            dispatch (CompleteTask(child.Id, task.Id)))
      ]
    ]

  let render (state: State) (dispatch: Msg -> unit) =
    let journeys =
      state.Settings.Children
      |> List.map (fun c ->
          let progress =
            state.Progress
            |> List.tryFind (fun p -> p.ChildId = c.Id)
          let total = c.Tasks.Length
          let completed =
            progress
            |> Option.map (fun p ->
                p.Tasks |> List.filter (fun t -> t.Completed) |> List.length)
            |> Option.defaultValue 0
          { Journey.Child = c
            Journey.Percent = State.childProgressPercent state c.Id
            Journey.Completed = completed
            Journey.Total = total })
    let childrenLabel =
      match state.Settings.Children with
      | [] -> "!"
      | [ c ] -> sprintf ", %s!" c.Name
      | [ a; b ] -> sprintf ", %s & %s!" a.Name b.Name
      | many ->
          let names = many |> List.map (fun c -> c.Name)
          let last = List.last names
          let head = names |> List.take (names.Length - 1) |> String.concat ", "
          sprintf ", %s & %s!" head last

    let header =
      Html.div [
        Html.h1 [
          prop.className "text-2xl md:text-3xl font-extrabold text-gray-800 drop-shadow-sm"
          prop.children [
            Html.span [ prop.text "Good Morning" ]
            Html.span [ prop.className "hidden md:inline"; prop.text childrenLabel ]
            Html.span [ prop.className "ml-2 hidden md:inline"; prop.text "☀️" ]
          ]
        ]
        Html.p [
          prop.className "text-gray-700 font-medium drop-shadow-sm"
          prop.text "Let's crush this morning together!"
        ]
      ]
    Html.div [
      prop.className "space-y-4"
      prop.children [
        Journey.render header journeys

        timerBar state

        Html.div [
          prop.className "grid grid-cols-1 md:grid-cols-2 gap-3"
          prop.children [
            for child in state.Settings.Children do
              childTaskList state dispatch child
          ]
        ]

        startStopBar state dispatch
      ]
    ]
