module Version.Console.Arguments

open System
open Argu
open Utils.FileSystem.Helpers
open Utils.FileSystem.Types
open Version.DotNet
open WhatsYourVersion

type ArgumentResult =
    | UpdateValues of TargetUpdate * string list
    | GetMeta of string

let curVersion =
    let assemblyWrapper = AssemblyWrapper.From<ArgumentResult>()
    let versionGetter = VersionRetriever (assemblyWrapper)
    let v = versionGetter.GetVersion()
    $"{v.Version} built at {v.BuildDateUtc}"
    
type CommandArguments =
    | [<Unique>][<AltCommandLine("-v")>] Version
    | [<Unique>][<AltCommandLine("-set-version")>]Set_Version of version:string
    | [<Unique>][<AltCommandLine("-major")>] Major
    | [<Unique>][<AltCommandLine("-minor")>] Minor
    | [<Unique>][<AltCommandLine("-patch")>] Patch
    | [<Unique>][<AltCommandLine("-subpatch")>][<AltCommandLine("-buildNum")>] SubPatch
    | [<Unique>][<AltCommandLine("-nonew")>] Dont_Add
    | [<Unique>][<AltCommandLine("-pre")>][<AltCommandLine("-prerelease")>] Prerelease of string
    | [<Unique>][<MainCommand>] Projects of string list
    interface IArgParserTemplate with
        member this.Usage =
            match this with
            | Version -> $"This will show the current version ({curVersion}) of the application"
            | Set_Version _ -> "Allows you to set the version to a specific value. This version prevents any other version commands from running. This will always create a new version if none are present."
            | Major -> "This argument will increment the major version number. This will cause Minor, Patch, and Sub Patch to be set to 0."
            | Minor -> "This argument will increment the minor version number. This will cause Patch and Sub Patch to be set to 0."
            | Patch -> "This argument will increment the patch version number. This will cause Sub Patch to be set to 0."
            | SubPatch -> "This argument will increment the sub-patch version or build number."
            | Prerelease _ -> "Sets the pre-release identifier, causing the library to be a prerelease."
            | Projects _ -> "Gets the project folders to look in for version information."
            | Dont_Add -> "Prevents the initialization of version for projects that do not have a version."

let getValue value =
    match value with
    | Some _ -> Increment |> Some
    | _ -> None
    
let setWhole versionString =
    let v = Parser.parseVersion String.Empty versionString 
    {
        MajorValue = SetValue v.Major |> Some
        MinorValue = SetValue v.Minor |> Some
        PatchValue = SetValue v.Patch |> Some
        SubPatchValue = SetValue v.SubPatch |> Some
        PrereleaseIdValue = v.PrereleaseId |> Some
        CreateNew = true
    }
    
let private getVersionUpdateInfo rootDir (fs: #IFileSystemAccessor) (results: ParseResults<CommandArguments>) =
    let setVersion = results.TryGetResult Set_Version
        
    let major =
        if results.Contains Major then
            Increment |> Some 
        else None
        
    let minor =
        if results.Contains Minor then
            Increment |> Some
        else None
        
    let patch =
        if results.Contains Patch then
            Increment |> Some
        else None
        
    let subPatch =
        if results.Contains SubPatch then
            Increment |> Some
        else None
        
    let prerelease =
        if results.Contains Prerelease then
            results.GetResult Prerelease |> Some
        else None
        
    let createNew = not <| results.Contains Dont_Add
        
    let projects =
        let p = 
            if results.Contains Projects then
                results.GetResult Projects
            else "."::[]
            
        p
        |> List.map (fs.JoinD rootDir)
        |> List.filter exists
        |> List.map getFullName
        
    match setVersion with
    | Some v -> v |> setWhole, projects
    | _ ->
        {
            MajorValue = major
            MinorValue = minor
            PatchValue = patch
            SubPatchValue = subPatch
            PrereleaseIdValue = prerelease
            CreateNew = createNew
        }, projects

let parseArgs rootDir fs (args: string []) =
    let parser = ArgumentParser.Create<CommandArguments>(programName = "Version")
    let results = parser.ParseCommandLine(inputs = args, raiseOnUsage = false)
    
    if results.Contains Version then
        curVersion
        |> GetMeta
    elif results.IsUsageRequested then
        parser.PrintUsage ()
        |> GetMeta
    else
        results
        |> getVersionUpdateInfo rootDir fs
        |> UpdateValues
    
