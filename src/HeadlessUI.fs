namespace App

module HeadlessUI =
  open Fable.Core
  open Fable.Core.JsInterop
  open Fable.React

  let [<Literal>] HeadlessUIPath = "@headlessui/react"

  module Combobox =
    type Props =
      | As of string
      | Value of string
      | Children of ReactElement seq
      | OnChange of (string -> unit)

    let Component (props: Props list) =
      ofImport "Combobox" HeadlessUIPath (keyValueList CaseRules.LowerFirst props) []

    module Input =

      type Props =
        | ClassName of string
        | OnChange of (Browser.Types.UIEvent -> unit)
        | OnBlur of (unit -> unit)
        | DisplayValue of string

      let Component (props: Props list) =
        ofImport "ComboboxInput" HeadlessUIPath (keyValueList CaseRules.LowerFirst props) []

    module Button =
      type Props =
        | ClassName of string
        | Children of ReactElement seq

      let Component (props: Props list) =
        ofImport "ComboboxButton" HeadlessUIPath (keyValueList CaseRules.LowerFirst props) []

    module Option =
      type Props =
        | Key of string
        | Value of string
        | ClassName of string
        | Children of ReactElement seq

      let Component (props: Props list) =
        ofImport "ComboboxOption" HeadlessUIPath (keyValueList CaseRules.LowerFirst props) []

    module Options =
      type Props =
        | ClassName of string
        | Children of ReactElement seq

      let Component (props: Props list) =
        ofImport "ComboboxOptions" HeadlessUIPath (keyValueList CaseRules.LowerFirst props) []