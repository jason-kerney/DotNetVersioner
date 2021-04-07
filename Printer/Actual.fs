module Utils.Printer.Actual

open Utils.Printer.Types

type Printer () =
    interface IPrinter with
        member __.PrintF format = printf format
        member __.PrintFn format = printfn format