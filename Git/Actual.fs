module Utils.Git.Actual

open Utils.FileSystem
open Utils.Git

type GitActual (fs: IFileSystemAccessor, rootDir: string, repositoryName: string) =
    let startEvent  = Event<GitStartEventDelegate, GitEventArgs>()
    let endEvent = Event<GitEndEventDelegate, GitEventArgs>()
    
    interface IGitStatusReporter with
        member __.GetCurrentBranchName () = Api.localBranch fs rootDir repositoryName
        member __.GetStatus () = Api.getStatus fs rootDir repositoryName
        member __.HasChanges () = Api.hasChanges fs rootDir repositoryName
    interface IGitUpdate with
        [<CLIEvent>]
        member __.UpdateStarted = startEvent.Publish
        [<CLIEvent>]
        member __.UpdateEnded = endEvent.Publish
        member this.Fetch () =
            let eventDetail = $"{repositoryName |> fs.JoinDirectoryPath rootDir} git fetch" |> EventDetails
            startEvent.Trigger (this, eventDetail)
            let result = Api.fetch fs rootDir repositoryName
            endEvent.Trigger (this, eventDetail)
            result
        member this.CloneFrom url =
            let eventDetail = $"{repositoryName |> fs.JoinDirectoryPath rootDir} git clone {url}" |> EventDetails
            startEvent.Trigger (this, eventDetail)
            let result = Api.clone fs rootDir repositoryName url
            endEvent.Trigger (this, eventDetail)
            result
    interface IGitModifier with
        member __.Tag tagName = Api.tag fs rootDir repositoryName tagName
        member __.CommitChanges comment = Api.commitChanges fs rootDir repositoryName comment 
    member this.StatusReporter with get() = this :> IGitStatusReporter
    member this.Update with get() = this :> IGitUpdate
    member this.Modifier with get () = this :> IGitModifier