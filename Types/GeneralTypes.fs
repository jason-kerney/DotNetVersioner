[<AutoOpen>]
module Version.DotNet.Types.GeneralTypes

open Utils.FileSystem
open Utils.Printer
    
type ProcessEnvironment =
    {
        Printer: IPrinter
        FileSystem: IFileSystemAccessor
        RootPath: string
    }
    interface IFileSystemAccessor with
        member this.FullFilePath path = path |> this.FileSystem.FullFilePath
        member this.File path = this.FileSystem.File path
        member this.MFile path = this.FileSystem.MFile path
        member this.FullDirectoryPath path = path |> this.FileSystem.FullDirectoryPath
        member this.Directory path = this.FileSystem.Directory path
        member this.MDirectory path = this.FileSystem.MDirectory path
        member this.JoinFilePath path fileName = fileName |> this.FileSystem.JoinFilePath path
        member this.MJoinFilePath path fileName = fileName |> this.FileSystem.MJoinFilePath path
        member this.JoinF path fileName = this.FileSystem.JoinF path fileName
        member this.MJoinF path fileName = this.FileSystem.MJoinF path fileName
        member this.JoinFD path fileName = this.FileSystem.JoinFD path fileName
        member this.MJoinFD path fileName = this.FileSystem.MJoinFD path fileName
        member this.JoinDirectoryPath path childFolder = childFolder |> this.FileSystem.JoinDirectoryPath path
        member this.MJoinDirectoryPath path childFolder = childFolder |> this.FileSystem.MJoinDirectoryPath path
        member this.JoinD path childFolder = this.FileSystem.JoinD path childFolder
        member this.MJoinD path childFolder = this.FileSystem.MJoinD path childFolder
            
    interface IPrinter with
        member this.PrintF format = this.Printer.PrintF format
        member this.PrintFn format = this.Printer.PrintFn format
        
    member this.File path = this.FileSystem.File path
    member this.Directory path = this.FileSystem.Directory path
    member this.JoinF path fileName = this.FileSystem.JoinF path fileName
    member this.JoinFD path fileName = this.FileSystem.JoinFD path fileName
    member this.JoinD childFolder path = this.FileSystem.JoinD childFolder path
    member this.MJoinD childFolder path = (this :> IFileSystemAccessor).MJoinD childFolder path
    member this.PrintF format = this.Printer.PrintF format
    member this.PrintFn format = this.Printer.PrintFn format
    member this.RootDir with get () = this.RootPath |> this.Directory
