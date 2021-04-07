module Utils.FileSystem.Actual.Accessor

open Utils.FileSystem.Builder
open Utils.Maybe
open Utils.FileSystem
open Utils.FileSystem.Helpers.FileSystem
open System.IO

type ActualFileSystemAccessor (fileEventHandlers: FileEventHandlers, dirEventHandlers: DirectoryEventHandlers) =
    interface IFileSystemAccessor with
        member this.FullFilePath path = path |> this.AsFileSystem.File |> getFullName
        member __.File path = path |> getFile fileEventHandlers dirEventHandlers
        member this.MFile path = path |> Maybe.lift this.AsFileSystem.File
        member this.FullDirectoryPath path = path |> this.AsFileSystem.Directory |> getFullName
        member __.Directory path = path |> getDirectory fileEventHandlers dirEventHandlers
        member this.MDirectory path = path |> Maybe.lift this.AsFileSystem.Directory
        member this.JoinFilePath path fileName =
            let fs = this.AsFileSystem
            fileName
            |> fs.JoinF path
            |> getFullName
            
        member this.MJoinFilePath path fileName =
            let fs = this.AsFileSystem
            maybe {
                let! path = path
                let! fileName = fileName
                
                return fileName |> fs.JoinFilePath path
            }
            
        member this.JoinF path fileName =
            let fs = this.AsFileSystem
            let fullPath =
                sprintf "%s%c%s" path Path.DirectorySeparatorChar fileName
            fullPath |> fs.File
            
        member this.MJoinF path fileName =
            let joinF = path |> Maybe.lift this.AsFileSystem.JoinF
            fileName /-> joinF
            
        member this.JoinFD (directory: IDirectoryWrapper) (fileName: string) =
            fileName |> this.AsFileSystem.JoinF directory.FullName 
            
        member this.MJoinFD directory fileName =
            let joinFD = directory |> Maybe.lift this.AsFileSystem.JoinFD
            fileName /-> joinFD
            
        member this.JoinDirectoryPath root childFolder =
            let fs = this.AsFileSystem
            childFolder
            |> fs.JoinD root
            |> getFullName
            
        member this.MJoinDirectoryPath root childFolder =
            let fs = this.AsFileSystem
            maybe {
                let! root = root
                let! childFolder = childFolder
                
                return childFolder |> fs.JoinDirectoryPath root
            }
        
        member this.JoinD root childFolder =
            let fs = this.AsFileSystem
            let fullPath =
                sprintf "%s%c%s" root Path.DirectorySeparatorChar childFolder
                
            fullPath |> fs.Directory

        member this.MJoinD root childFolder =
            let joinD = root |> Maybe.lift this.AsFileSystem.JoinD
            childFolder /-> joinD
            
    member this.AsFileSystem with get () = this :> IFileSystemAccessor
