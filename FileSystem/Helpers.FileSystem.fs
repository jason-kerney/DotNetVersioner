[<AutoOpen>]
module Utils.FileSystem.Helpers.FileSystem

open Utils.Maybe
open Utils.FileSystem
    
let maybeGetFullName (fs: #IFileSystemWrapper maybe) =
    maybe {
        let! fs = fs
        return fs.FullName
    }
    
let getFullName (fs: #IFileSystemWrapper) = fs.FullName
    
let exists (fs: #IFileSystemWrapper) = fs.Exists
    
let maybeExists (fs: #IFileSystemWrapper maybe) =
    maybe {
        let! fs = fs
        return fs.Exists
    }
    |> Maybe.toBool