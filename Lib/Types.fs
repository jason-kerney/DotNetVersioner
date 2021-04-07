[<AutoOpen>]
module Version.DotNet.Types

open Utils.FileSystem.Types
open Utils.Maybe

let private dot = "."

type Version =
    {
        Major : int
        Minor : int
        Patch : int
        SubPatch : int
        PrereleaseId: string
    }
    member this.ToMainVersionString() =
        $"{this.Major}.{this.Minor}.{this.Patch}.{this.SubPatch}"

    member this.ToFullVersionString () =
        $"{this.ToMainVersionString ()}{if 0 < this.PrereleaseId.Length then dot else System.String.Empty}{this.PrereleaseId}"
    override this.ToString() =
        this.ToFullVersionString ()
        
let emptyVersion =
    { Major = 0; Minor = 0; Patch = 0; SubPatch = 0; PrereleaseId = System.String.Empty }
    
type IFinder =
    abstract member Name : string with get
    abstract member Directory: IDirectoryWrapper with get
    abstract member GetVersion : unit -> Version maybe

type IRemovable =
    inherit IFinder
    abstract member ClearVersion : unit -> unit maybe
    
type IUpdatable =
    inherit IFinder
    abstract member UpdateVersion : create:bool -> Version -> Version maybe
    
type VersionUpdateType =
    | Increment
    | SetValue of int
    
type TargetUpdate =
    {
        MajorValue : VersionUpdateType option
        MinorValue : VersionUpdateType option
        PatchValue : VersionUpdateType option
        SubPatchValue : VersionUpdateType option
        PrereleaseIdValue : string option
        CreateNew: bool
    }
    
let emptyUpdate =
    { MajorValue = None; MinorValue = None; PatchValue = None; SubPatchValue = None; PrereleaseIdValue = None; CreateNew = true }
    
let internal hasValue possible =
    match possible with
    | Some _ -> true
    | _ -> false
    
let internal hasValueOf check possible =
    match possible with
    | Some v -> v = check
    | _ -> false
    
let private tryGetUpdateType possible =
    match possible with
    | Some value -> true, value
    | None -> false, Increment
    
let prefer preferred possible  =
    match preferred with
    | Some value -> value
    | _ -> possible
    
let private updateValue updateType value =
    let (hasUpdate, updateType) = tryGetUpdateType updateType
    if not <| hasUpdate then value
    else
        match updateType with
        | Increment -> value + 1
        | SetValue v -> v

let updateVersion (update: TargetUpdate) (version: Version) =
    let updateMajorTarget updateTarget =
        if updateTarget.MajorValue |> hasValueOf Increment then
            {updateTarget with
                MinorValue = 0 |> SetValue |> Some
                PatchValue = 0 |> SetValue |> Some
                SubPatchValue = 0 |> SetValue |> Some
            }
            
        else updateTarget
        
    let updateMinorTarget updateTarget =
        if updateTarget.MinorValue |> hasValueOf Increment then
            {updateTarget with
                PatchValue = 0 |> SetValue |> Some
                SubPatchValue = 0 |> SetValue |> Some
            }
        else updateTarget
        
    let updatePatchTarget updateTarget =
        if updateTarget.PatchValue |> hasValueOf Increment then
            {updateTarget with
                SubPatchValue  = 0 |> SetValue |> Some
            }
        else updateTarget
        
    let validTarget = update |> updateMajorTarget |> updateMinorTarget |> updatePatchTarget
    let create = update.CreateNew
    
    {version with
        Major = version.Major |> updateValue validTarget.MajorValue
        Minor = version.Minor |> updateValue validTarget.MinorValue
        Patch = version.Patch |> updateValue validTarget.PatchValue
        SubPatch = version.SubPatch |> updateValue validTarget.SubPatchValue
        PrereleaseId = version.PrereleaseId |> prefer validTarget.PrereleaseIdValue
    }, create