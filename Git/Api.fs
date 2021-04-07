module Utils.Git.Api

open System
open Utils.FileSystem.Types
open Utils.Maybe
open Utils.Printer
open Utils.Git
open Utils.Git.Process
open Utils.Strings

type GitTag = {
    Name : string
    Sha : string
}

let gitNow getArgs (fs: #IFileSystemAccessor) rootDir repositoryName =
    let target =
        repositoryName
        |> fs.JoinDirectoryPath rootDir
    
    maybe {
        let! args = repositoryName |> getArgs
        return!
            start target "git" args
            |> Async.RunSynchronously
    }
    
let gitNowResult getArgs (fs: #IFileSystemAccessor) rootDir repository =
    maybe {
        let! (_, result) =
            gitNow getArgs fs rootDir repository
            
        return result
    }
    
let internal printResults (printer: IPrinter) (asyncResults: (string * string) maybe Async list) =
    let divider = "\n-------------------------\n"
    let results =
        asyncResults
        |> Async.Parallel
        |> Async.RunSynchronously
        
    results
    |> Array.map (fun r ->
        match r with
        | Error e -> "", e |> Error |> sprintf "%A"
        | Ok v -> v
    )
    |> Array.filter (fun (_, v) -> 0 < v.Length)
    |> Array.map (fun (id, v) -> v |> sprintf "%s %s" id |> trim)
    |> joinBys divider
        |> printer.PrintFn "%s%s" divider
    
let localBranch (fs: #IFileSystemAccessor) rootDir repositoryName =
    gitNowResult (fun _ -> Ok "rev-parse --abbrev-ref HEAD") fs rootDir repositoryName
    
let internal getBranchName format (fs: #IFileSystemAccessor) rootDir repositoryName =
    maybe {
        let! branchName = localBranch fs rootDir repositoryName
        return
            sprintf format branchName
    }
    
let internal originShaw (fs: #IFileSystemAccessor) rootDir repositoryName =
    let getArgs _ = getBranchName "rev-parse origin/%s" fs rootDir repositoryName
    gitNowResult getArgs fs rootDir repositoryName
    
let internal localShaw (fs: #IFileSystemAccessor) rootDir repositoryName =
    let getArgs _ = getBranchName "rev-parse %s" fs rootDir repositoryName
    gitNowResult getArgs fs rootDir repositoryName
    
let internal baseShaw fs rootDir repositoryName =
    let getArgs _ = getBranchName "merge-base HEAD origin/%s" fs rootDir repositoryName
    gitNowResult getArgs fs rootDir repositoryName
    
let internal hasUncommittedChanges  (fs: #IFileSystemAccessor) rootDir repositoryName =
    let getArgs _ = Ok "status -s"
    
    repositoryName
    |> gitNowResult getArgs fs rootDir
    |> Maybe.check (fun s -> 0 < s.Length)
    
let internal asSeq (matches : System.Text.RegularExpressions.MatchCollection) =
    seq {
        for m in matches do
            yield m
    }
    
let private parseTags resultString =
    let regex = System.Text.RegularExpressions.Regex @"^(?<sha>\w+)\s+(?<tag>[a-z/\d\.-]+)\s*$"

    resultString
    |> msplit
    |> MaybeList.map (regex.Matches >> asSeq >> Seq.toList)
    |> MaybeList.concat
    |> MaybeList.map (fun m ->
        {
            Sha = m.Groups.["sha"].Value
            Name = m.Groups.["tag"].Value
        }
    )
    |> MaybeList.sortBy (fun tag -> tag.Name)
    
let internal remoteTags (fs: #IFileSystemAccessor) rootDir repositoryName =
    let getArgs = fun _ -> Ok "ls-remote --tags origin"
    
    repositoryName
    |> gitNowResult getArgs fs rootDir
    |> parseTags
    
let internal localTags (fs: #IFileSystemAccessor) rootDir repositoryName =
    let getArgs = fun _ -> Ok "show-ref --tags"
    
    repositoryName
    |> gitNowResult getArgs fs rootDir
    |> parseTags
    
let hasNewTags (toCompare: GitTag mlist) (source: GitTag mlist) =
    maybe {
        let! toCompare = toCompare
        let! source = source
        
        return 0 < (source |> List.except toCompare |> List.length)
    }
    
let getTagDifferences fs rootDir repositoryName =
    let localTags = localTags fs rootDir repositoryName
    let remoteTags = remoteTags fs rootDir repositoryName
    
    (localTags |> hasNewTags remoteTags) |> Maybe.toBool, (remoteTags |> hasNewTags localTags) |> Maybe.toBool
    
let getStatus (fs: #IFileSystemAccessor) rootDir repositoryName =
    let result =
        maybe {
            let! originShaw = originShaw fs rootDir repositoryName
            let! localShaw = localShaw fs rootDir repositoryName
            let! baseShaw = baseShaw fs rootDir repositoryName
            let hasChanges = hasUncommittedChanges fs rootDir repositoryName
            let hasLocalTags, hasRemoteTags = getTagDifferences fs rootDir repositoryName
            
            let upToDate =
                originShaw = localShaw
            let needsPull =
                (not upToDate) && localShaw = baseShaw
            let needsPush =
                (not upToDate) && originShaw = baseShaw
            let needsMerge =
                needsPull && needsPush
            
            return
                if hasChanges then repositoryName |> UncommittedChanges
                elif needsMerge then repositoryName |> Merge
                elif needsPull then repositoryName |> Pull
                elif needsPush then repositoryName |> Push 
                elif hasLocalTags then repositoryName |> PushTags
                elif hasRemoteTags then repositoryName |> PullTags
                else NoAction
        }
    
    match result with
    | Ok v -> v
    | _ -> NoAction
        
let hasChanges (fs: #IFileSystemAccessor) rootDir repositoryName =
    let need = getStatus fs rootDir repositoryName
    not <| (need = NoAction)
    
let fetch (fs: #IFileSystemAccessor) rootDir (repositoryName: string) =
    let target = repositoryName |> fs.JoinDirectoryPath rootDir
    
    start target "git" "fetch"
    
let clone (fs: #IFileSystemAccessor) rootDir (repositoryName: string) (repositoryUrl: string) =
    let clone = sprintf "clone %s" repositoryUrl
    let target = repositoryName |> fs.JoinDirectoryPath rootDir
    start target "git" clone
    
let tag (fs: #IFileSystemAccessor) rootDir repositoryName tagName =
    let now = DateTime.UtcNow.ToString ("ddd, dd MMM yyy HH’:’mm’:’ss ‘UTC’")
    let tag = $"tag {tagName}  -m \"{tagName} Version Tool @ {now}\""
    let target = repositoryName |> fs.JoinDirectoryPath rootDir
    start target "git" tag
    
let commitChanges (fs: #IFileSystemAccessor) rootDir repositoryName (comment: string) =
    let commit = $"commit -am \"{comment}\""
    let target = repositoryName |> fs.JoinDirectoryPath rootDir
    start target "git" commit 