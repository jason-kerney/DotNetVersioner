module Version.Console.Program

open Version.Console.Arguments
open Utils.FileSystem.Builder
open Utils.FileSystem.Helpers
open Utils.FileSystem.Types
open Utils.Git
open Utils.Maybe
open Utils.Strings
open Version.DotNet
open Version.DotNet.Finders.BuildProps

let private andFork f value =
    value, f value
    
let private fork f value =
    f value |> ignore
    value
    
let updateValues (fs: #IFileSystemAccessor) dirs update =
    let create =  update.CreateNew
    let mapping =
        dirs
        |> List.map ((getBuildPropsVersionFinder fs) )
        
    let updateVersion = update |> Ok |> Maybe.lift updateVersion |> apply 
    
    let getVersionOrDefault (finder: #IFinder) =
        match finder.GetVersion () with
        | Ok v -> finder, (Ok v)
        | _ ->
            let v =
                if create then (Ok emptyVersion)
                else "No Version Found" |> asGeneralFailure
            finder, v
        
    let versions =
        mapping
        |> List.map getVersionOrDefault
        |> List.map (fun (m, v) -> (m, v |> updateVersion))
    
    let result = 
        versions
        |> List.map (fun (m, v) ->
            m.Name, maybe {
                let! (v, create) = v
                return! m.UpdateVersion create v
            }
        )
        
    let tags =
        result
        |> List.filter (fun (_, v) -> v |> Maybe.hasValue)
        |> List.map (fun (fileName, Ok (v)) ->
            let file = fs.File fileName
            let dir = file.Path
            let name = dir.Name
            
            $"{name}_{v.ToFullVersionString ()}")
        
    let tag =
        tags
        |> joinByString "; "
        
    let baseDir = fs.Directory "."
    let root = baseDir.Parent |> getFullName
    let repositoryName = baseDir.Name
    let git = getModifier fs root repositoryName
    
    let toResultString = function
        | Ok (action, result) -> $"{action}:\n{result}\n"
        | Error e -> e |> Error |> sprintf "%A"
    
    let commitResult =
        git.CommitChanges tag
        |> Async.RunSynchronously
        |> toResultString
    
    let tagResult =
        tags
        |> List.map (git.Tag)
        |> Async.Parallel
        |> Async.RunSynchronously
        |> Array.map toResultString
        |> Array.reduce (sprintf "%s\n%s")
        
    result, [commitResult; tagResult]

[<EntryPoint>]
let main argv =
    let fs = getPlainFileSystem ()
    let currentDirectory =
        "."
        |> fs.FullDirectoryPath
        
    let args = parseArgs currentDirectory fs argv
    
    let baseDir = fs.Directory "."
    let root = baseDir.Parent |> getFullName
    let repositoryName = baseDir.Name
    let git = getReporter fs root repositoryName
    
    let status = git.GetStatus ()
    
    match args with
    | GetMeta meta ->
        printfn "%s" meta
        0
    | UpdateValues (update, dirs) ->
        match status with
        | PushTags _
        | Push _
        | NoAction -> 

            let result, gitResult = updateValues fs dirs update
            
            let outPut =
                result
                |> List.map (fun (s, v) -> s, v |> Maybe.callToString)

            gitResult
            |> List.iter (printfn "%s")

            printfn "%A" outPut
            
            ()
        
            0
        | error ->
            printfn "%A" error
            -1
