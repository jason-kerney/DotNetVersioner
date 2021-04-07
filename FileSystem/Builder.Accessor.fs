[<AutoOpen>]
module Utils.FileSystem.Builder.Accessor

open Utils.FileSystem
open Utils.FileSystem.Actual.Accessor
    
let getFileSystem fileEventHandlers dirEventHandlers =
    ActualFileSystemAccessor (fileEventHandlers, dirEventHandlers)
    :> IFileSystemAccessor

let getPlainFileSystem () =
    getFileSystem emptyFileEventHandler emptyDirectoryEventHandler