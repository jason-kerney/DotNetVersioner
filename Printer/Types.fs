[<AutoOpen>]
module Utils.Printer.Types

type IPrinter =
    abstract member PrintF: Printf.TextWriterFormat<('a)> -> 'a
    abstract member PrintFn: Printf.TextWriterFormat<('a)> -> 'a