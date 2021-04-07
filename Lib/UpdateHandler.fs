namespace Version.DotNet

open Utils.Maybe
open Version.DotNet.UpdateCombiner
open Utils.FileSystem.Types

type UpdateHandler (fs: IFileSystemAccessor, finders: IFinder list) =
    member this.UpdateVersions (update: TargetUpdate) =
        let getVersion (finder: #IFinder) =  finder, finder.GetVersion ()
        let updateVersion (updater: IUpdatable, maybeVersion) =
            updater, maybe {
                let! (newVersion, create) = maybeVersion |> Maybe.lift (updateVersion update)
                return! newVersion |> updater.UpdateVersion create
            }
        let updaters =
            finders
            |> List.map (toUpdater fs)
            
        let versions =
            updaters
            |> List.map getVersion
            
        let versionResults =
            versions
            |> List.map updateVersion
        
        versionResults