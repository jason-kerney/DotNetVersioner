[<AutoOpen>]
module Utils.FileSystem.Types

open Utils.Maybe
    
type IFileSystemWrapper =
    abstract member FullName: string with get
    abstract member Exists: bool with get
    abstract member Delete: unit -> Async<maybe<unit>>

type IFileWrapper =
    inherit IFileSystemWrapper
    abstract member Path: IDirectoryWrapper with get
    abstract member ReadAllText: unit -> maybe<string>
    abstract member WriteAllText: text:maybe<string> -> maybe<unit>

and IDirectoryWrapper =
    inherit IFileSystemWrapper
    abstract member Name: string with get
    abstract member GetFiles: pattern:string -> IFileWrapper mlist
    abstract member GetFiles: unit -> IFileWrapper mlist
    abstract member GetDirectories: unit -> IDirectoryWrapper mlist
    abstract member Parent : IDirectoryWrapper with get
    
type IFileSystemAccessor =
    abstract member FullFilePath: path: string -> string
    abstract member File: path:string -> IFileWrapper
    abstract member MFile: path:string maybe -> IFileWrapper maybe
    
    abstract member FullDirectoryPath: path:string -> string
    abstract member Directory: path:string -> IDirectoryWrapper
    abstract member MDirectory: path:string maybe -> IDirectoryWrapper maybe
    
    abstract member JoinFilePath: path:string -> fileName:string -> string
    abstract member MJoinFilePath: path:string maybe -> fileName:string maybe -> string maybe
    
    abstract member JoinF: path:string -> fileName:string -> IFileWrapper
    abstract member MJoinF: path:string maybe -> fileName:string maybe -> IFileWrapper maybe
    
    abstract member JoinFD: directory:IDirectoryWrapper -> fileName:string -> IFileWrapper
    abstract member MJoinFD: directory:IDirectoryWrapper maybe -> fileName:string maybe -> IFileWrapper maybe
    
    abstract member JoinDirectoryPath: root:string -> childFolder:string -> string
    abstract member MJoinDirectoryPath: root: string maybe -> childFolder:string maybe -> string maybe
    
    abstract member JoinD: root:string -> childFolder:string -> IDirectoryWrapper
    abstract member MJoinD: root:string maybe -> childFolder:string maybe -> IDirectoryWrapper maybe
    
type FileEventHandlers = {
    BeforeDelete : (IFileWrapper -> unit) option
    AfterDelete : (IFileWrapper -> unit) option
}

type DirectoryEventHandlers = {
    BeforeDelete : (IDirectoryWrapper -> unit) option
    AfterDelete : (IDirectoryWrapper -> unit) option
}

[<AutoOpen>]
module BaseTypes =
    let emptyFileEventHandler : FileEventHandlers =
        {
            BeforeDelete = None
            AfterDelete = None
        }
        
    let emptyDirectoryEventHandler : DirectoryEventHandlers =
        {
            BeforeDelete = None
            AfterDelete = None
        }
        
    let addFileBeforeDelete f (handlers: FileEventHandlers) =
        {handlers with
            BeforeDelete = Some f
        }
        
    let addFileAfterDelete f (handlers: FileEventHandlers) =
        {handlers with
            AfterDelete = Some f
        }
        
    let addDirectoryBeforeDelete f (handlers: DirectoryEventHandlers) =
        {handlers with
            BeforeDelete = Some f
        }
        
    let addDirectoryAfterDelete f (handlers: DirectoryEventHandlers) =
        {handlers with
            AfterDelete = Some f
        }