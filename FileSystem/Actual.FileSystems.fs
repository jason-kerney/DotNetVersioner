module Utils.FileSystem.Actual.FileSystems

open System.IO
open Utils.Maybe
open Utils.FileSystem

let internal noOp (_fileSystem: #IFileSystemWrapper) = ()

let internal getHandler fOption =
    match fOption with
    | Some f -> f
    | None -> noOp

type ActualFile (path, fileEventHandlers: FileEventHandlers, dirEventHandlers: DirectoryEventHandlers) =
    let before = fileEventHandlers.BeforeDelete |> getHandler
    let after = fileEventHandlers.AfterDelete |> getHandler
        
    interface IFileWrapper with
        member this.FullName
            with get () =
                let info = this.GetInfo ()
                info.FullName
        member this.Exists
            with get () =
                let info = this.GetInfo ()
                info.Exists
                
        member this.Delete () =
            this |> before
            async {
                return
                    maybe {
                        let info = this.GetInfo ()
                        if info.Exists then
                            info.Attributes <- FileAttributes.Normal
                            info.Delete ()
                            
                        this |> after
                    }
            }
            
        member this.Path
            with get () =
                let name = this.FullName
                let dir = Path.GetDirectoryName name
                (ActualDirectory (dir, fileEventHandlers, dirEventHandlers)) :> IDirectoryWrapper
                
        member this.ReadAllText () =
            let file = this.GetInfo ()
            if file.Exists then
                maybe {
                    return
                        file.FullName
                        |> File.ReadAllText
                }
            else
                file.FullName
                |> sprintf "%s does not exist, unable to read."
                |> asGeneralFailure
                
        member this.WriteAllText text =
            let file = this.AsFile ()
            maybe {
                let! text = text
                return 
                    File.WriteAllText (file.FullName, text)
            }
            
    member this.AsFile () = this :> IFileWrapper
    member this.FullName
        with get () =
            let file = this.AsFile ()
            file.FullName
            
    member this.Path
        with get () =
            let file = this.AsFile ()
            file.Path
            
    member __.GetInfo () =
        let info = FileInfo (path)
        info.Refresh ()
        info

and ActualDirectory (path, fileEventHandlers, dirEventHandlers) =
    let before = dirEventHandlers.BeforeDelete |> getHandler
    let after = dirEventHandlers.AfterDelete |> getHandler
    
    interface IDirectoryWrapper with
        member __.FullName
            with get () =
                let info = DirectoryInfo (path)
                info.FullName
                
        member __.Exists
            with get () =
                let info = DirectoryInfo (path)
                info.Refresh ()
                info.Exists
                
        member this.Delete () =
            this |> before
            let dir = this.AsDir ()
            
            async {
                return
                    maybe {
                        let directories = dir.GetDirectories ()
                        let files = dir.GetFiles ()
                     
                        let! fileDeletions =        
                            files
                            |> MaybeList.map (fun f -> f.Delete ())
                            
                        let! _success =
                            fileDeletions
                            |> Async.Parallel
                            |> Async.RunSynchronously
                            |> MaybeList.reduceErrors
                            
                        let! directoryDeletions =
                            directories
                            |> MaybeList.map (fun d -> d.Delete ())
                            
                        let! _success =
                            directoryDeletions
                            |> Async.Parallel
                            |> Async.RunSynchronously
                            |> MaybeList.reduceErrors
                            
                        let info = DirectoryInfo (dir.FullName)
                        info.Delete ()
                        this |> after
                            
                        return ()
                    }
            }
            
        member __.Name
            with get () =
                let info = DirectoryInfo (path)
                info.Name
                
        member __.GetFiles pattern =
            let info = DirectoryInfo (path)
            if info.Exists then
                maybe {
                    return
                        pattern
                        |> info.GetFiles
                        |> Array.toList
                        |> List.map (fun i -> ActualFile (i.FullName, fileEventHandlers, dirEventHandlers) :> IFileWrapper)
                }
            else
                info.FullName
                |> sprintf "%s does not exist and does not have files to filter"
                |> asGeneralFailure
                
        member __.GetFiles () =
            let info = DirectoryInfo (path)
            if info.Exists then
                maybe {
                    return
                        ()
                        |> info.GetFiles
                        |> Array.toList
                        |> List.map (fun i -> ActualFile (i.FullName, fileEventHandlers, dirEventHandlers) :> IFileWrapper)
                }
            else
                info.FullName
                |> sprintf "%s does not exist and does not have files"
                |> asGeneralFailure
                
        member __.GetDirectories () =
            let info = DirectoryInfo (path)
            if info.Exists then
                maybe {
                    return
                        ()
                        |> info.GetDirectories
                        |> Array.toList
                        |> List.map (fun i -> ActualDirectory (i.FullName, fileEventHandlers, dirEventHandlers) :> IDirectoryWrapper)
                }
            else
                info.FullName
                |> sprintf "%s does not exist and does not have children directories"
                |> asGeneralFailure
                
        member __.Parent
            with get () =
                let info = DirectoryInfo (path)
                
                ActualDirectory (info.Parent.FullName, fileEventHandlers, dirEventHandlers)
                :> IDirectoryWrapper
                
    member this.AsDir () = this :> IDirectoryWrapper
    
    member this.FullName
        with get () =
            let dir = this.AsDir ()
            dir.FullName
