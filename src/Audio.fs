namespace MorningRoutine.Components

module Audio =

  open System
  open Fable.Core
  open Fable.Core.JsInterop


  [<StringEnum; RequireQualifiedAccess>]
  type OscillatorType =
    | Sine
    | Square
    | Sawtooth
    | Triangle
    | Custom

  [<AllowNullLiteral>]
  type AudioParam =
    abstract setValueAtTime: value: float * time: float -> unit
    abstract exponentialRampToValueAtTime: value: float * endTime: float -> unit
    abstract linearRampToValueAtTime: value: float * endTime: float -> unit
    abstract value: float with get, set

  [<AllowNullLiteral>]
  type GainNode =
    abstract gain: AudioParam
    abstract connect: destination: obj -> unit

  [<AllowNullLiteral>]
  type OscillatorNode =
    abstract ``type``: OscillatorType with get, set
    abstract frequency: AudioParam
    abstract connect: destination: obj -> unit
    abstract start: ?whenTime: float -> unit
    abstract stop: ?whenTime: float -> unit

  [<AllowNullLiteral>]
  type AudioContext =
    abstract createOscillator: unit -> OscillatorNode
    abstract createGain: unit -> GainNode
    abstract destination: obj
    abstract currentTime: float

  [<Global("SpeechSynthesisUtterance")>]
  [<AbstractClass>]
  type SpeechSynthesisUtterance =
    [<Emit("new SpeechSynthesisUtterance($0)")>]
    static member create(text: string): SpeechSynthesisUtterance = jsNative
    abstract member rate: float with get, set
    abstract member pitch: float with get, set
    abstract member volume: float with get, set
    abstract member text: string with get, set

  type SpeechSynthesis =
    abstract member speak: utterance: SpeechSynthesisUtterance -> unit

  [<Global("window.speechSynthesis")>]
  let speechSynthesis : SpeechSynthesis = jsNative

  let createAudioContext (): AudioContext =
    emitJsExpr<AudioContext>() "new AudioContext()"

  let playBeep () =
    try
      // Check if AudioContext is supported
      if not (isNull Browser.Dom.window?AudioContext) then
        let context = createAudioContext()
        let oscillator = context.createOscillator()
        let gainNode = context.createGain()
        oscillator.connect(gainNode)
        gainNode.connect(context.destination)
        oscillator.frequency.value <- 800.0 // 800Hz tone
        gainNode.gain.value <- 0.1 // Low volume
        oscillator.start()
        oscillator.stop(context.currentTime + 0.5) // 500ms beep
      else
        printfn "Audio not supported"
    with
    | ex -> printfn "Error playing beep: %s" ex.Message

  let speakText text =
    try
      let utterance = SpeechSynthesisUtterance.create text
      utterance.rate <- 1.0 // Normal speed
      utterance.pitch <- 1.0 // Normal pitch
      utterance.volume <- 1.0 // Full volume
      speechSynthesis.speak(utterance)
    with
    | ex -> printfn "Error speaking text: %s" ex.Message

  let shouldPlayPrompt (remainingTime: TimeSpan) =
    let totalMinutes = int remainingTime.TotalMinutes
    let seconds = remainingTime.Seconds
    seconds = 0 && (totalMinutes = 15 || totalMinutes = 10 || totalMinutes = 5)

  /// Plays an audio prompt announcing the remaining minutes
  let playTimePrompt (remainingTime: TimeSpan) =
    if shouldPlayPrompt remainingTime then
      let minutes = int remainingTime.TotalMinutes
      playBeep()
      // delay to ensure beep plays before speaking
      // let handler = fun () -> speakText $"%i{minutes} minutes remaining"
      // Browser.Dom.window.setTimeout(handler, 1000) |> ignore
      
  /// Plays completion audio when time is up
  let playCompletionPrompt () =
    //playBeep()
    speakText "Time's up!"

  /// Alternative audio file player (if you want to use audio files instead)
  let playAudioFile audioUrl =
    try
      let audio = Browser.Dom.document.createElement("audio") :?> Browser.Types.HTMLAudioElement
      audio.src <- audioUrl
      audio.play() |> ignore
    with
    | ex -> printfn "Error playing audio file: %s" ex.Message
