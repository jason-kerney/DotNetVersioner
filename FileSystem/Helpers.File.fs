[<AutoOpen>]
module Utils.FileSystem.Helpers.File

open Utils.Maybe
open Utils.FileSystem

let maybePath (file: #IFileWrapper maybe) =
    maybe {
        let! file = file
        return file.Path
    }
    
let readAllText (file: #IFileWrapper) = file.ReadAllText ()
    
let maybeReadAllText (file: #IFileWrapper maybe) = file |> Maybe.partialLift readAllText 

let writeAllText (file: #IFileWrapper) text = text |> file.WriteAllText
    
let maybeWriteAllText (file: #IFileWrapper maybe) text =
    let writeAll = file |> Maybe.lift writeAllText
    text |-> writeAll