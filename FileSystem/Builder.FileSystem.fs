[<AutoOpen>]
module Utils.FileSystem.Builder.FileSystem

open Utils.FileSystem

open Actual.FileSystems
            
let getFile fileEventHandlers dirEventHandlers path =
    ActualFile (path, fileEventHandlers, dirEventHandlers) :> IFileWrapper
    
let getPlainFile = getFile emptyFileEventHandler emptyDirectoryEventHandler 
    
let getDirectory fileEventHandlers dirEventHandlers path =
    ActualDirectory (path, fileEventHandlers, dirEventHandlers) :> IDirectoryWrapper

let getPlainDirectory = getDirectory emptyFileEventHandler emptyDirectoryEventHandler