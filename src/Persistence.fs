namespace MorningRoutine

open System
open Thoth.Json
open MorningRoutine.Components.RoutineTracker

module Persistence =

  let private settingsKey = "morning.settings.v1"
  let private progressKey = "morning.progress.v1"
  let private timerKey = "morning.timer.v1"

  type private StoredProgress = {
    Date: string
    Progress: ChildProgress list
  }

  // The running timer is restored after a refresh from the absolute deadline
  // (epoch ms), so the countdown keeps ticking correctly even while the tab
  // was closed. RemainingSeconds is used for the frozen (paused) value.
  type StoredTimer = {
    Date: string
    Status: string
    DeadlineEpochMs: float option
    RemainingSeconds: float
  }

  let todayIso () : string =
    let now = DateTime.Now
    sprintf "%04d-%02d-%02d" now.Year now.Month now.Day

  let private setItem (key: string) (value: string) =
    try Browser.Dom.window.localStorage.setItem(key, value)
    with _ -> ()

  let private getItem (key: string) : string option =
    try
      let v = Browser.Dom.window.localStorage.getItem(key)
      if isNull v then None else Some v
    with _ -> None

  let saveSettings (settings: FamilySettings) =
    let json = Encode.Auto.toString(0, settings)
    setItem settingsKey json

  let loadSettings () : FamilySettings option =
    getItem settingsKey
    |> Option.bind (fun json ->
        match Decode.Auto.fromString<FamilySettings>(json) with
        | Ok v -> Some v
        | Error _ -> None)

  let saveProgress (date: string) (progress: ChildProgress list) =
    let json = Encode.Auto.toString(0, { Date = date; Progress = progress })
    setItem progressKey json

  let loadProgressFor (today: string) : ChildProgress list option =
    getItem progressKey
    |> Option.bind (fun json ->
        match Decode.Auto.fromString<StoredProgress>(json) with
        | Ok stored when stored.Date = today -> Some stored.Progress
        | _ -> None)

  let saveTimer (timer: StoredTimer) =
    let json = Encode.Auto.toString(0, timer)
    setItem timerKey json

  let loadTimerFor (today: string) : StoredTimer option =
    getItem timerKey
    |> Option.bind (fun json ->
        match Decode.Auto.fromString<StoredTimer>(json) with
        | Ok stored when stored.Date = today -> Some stored
        | _ -> None)
