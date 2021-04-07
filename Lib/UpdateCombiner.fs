module Version.DotNet.UpdateCombiner

open Utils.FileSystem.Helpers
open Utils.FileSystem.Types
open Utils.Maybe
open Version.DotNet.Finders.BuildProps

type ToUpdater (fs: IFileSystemAccessor, finder :IFinder) =
    let updater = finder.Directory |> getFullName |> getBuildPropsVersionFinder fs
    let mutable version = Ok emptyVersion
                  
    do
      match finder.GetVersion () with
      | Ok v ->
            if finder :? IRemovable then
                let removable = finder :?> IRemovable
                removable.ClearVersion () |> ignore
            v |> updater.UpdateVersion true |> ignore
            version <- Ok v
      | Error e -> version <- Error e
    
    interface IUpdatable with
        member __.Name = $"{finder.Name} ==> {updater.Name}"
        member __.Directory with get () = finder.Directory
        member __.GetVersion () = version |> Maybe.butPrefer (updater.GetVersion ())
        member __.UpdateVersion create version = updater.UpdateVersion create version
        
let toUpdater fs (finder: IFinder) =
    if finder :? IUpdatable then finder :?> IUpdatable
    else ToUpdater (fs, finder) :> IUpdatable