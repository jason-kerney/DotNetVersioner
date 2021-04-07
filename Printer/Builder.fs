[<AutoOpen>]
module Utils.Printer.Builder

open Utils.Printer.Types

let getPrinter () = Actual.Printer () :> IPrinter