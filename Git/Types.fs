[<AutoOpen>]
module Utils.Git.Types

open Utils.Maybe

type GitStatus =
    | UncommittedChanges of string
    | NoAction
    | Pull of string
    | Push of string
    | Merge of string
    | PushTags of string
    | PullTags of string

type GitEventArgs =
    | EventDetails of string
    
type GitResultEventArgs =
    | ResultArg of GitEventArgs * ((string * string) maybe)
    
type GitStartEventDelegate = delegate of obj * GitEventArgs -> unit
type GitEndEventDelegate = delegate of obj * GitEventArgs -> unit
    
type IGitStatusReporter =
    abstract member GetCurrentBranchName: unit -> string maybe
    abstract member GetStatus: unit -> GitStatus
    abstract member HasChanges: unit -> bool

type IGitUpdate =
    [<CLIEvent>]
    abstract member UpdateStarted: IEvent<GitStartEventDelegate, GitEventArgs>
    [<CLIEvent>]
    abstract member UpdateEnded: IEvent<GitEndEventDelegate, GitEventArgs>
    abstract member Fetch: unit -> (string * string) maybe Async
    abstract member CloneFrom: url:string -> (string * string) maybe Async
    
type IGitModifier =
    abstract member Tag: tagName:string -> (string * string) maybe Async
    abstract member CommitChanges: comment:string -> (string * string) maybe Async
    
type GitSystem =
    {
        Status: IGitStatusReporter
        Update: IGitUpdate
        Modifier: IGitModifier
    }