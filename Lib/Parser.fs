module Version.DotNet.Parser

open Version.DotNet.Types
open Utils.Strings

let getValueOrDefault defaultValue = function
    | Some v -> v
    | _ -> defaultValue
    
let parseVersion suffix version =
    let parts =
        version
        |> splitBy [|'.'|]
        
    let intParts =
        parts
        |> List.map System.Int32.TryParse
        |> List.filter fst
        |> List.map snd
        
    {
        Major = intParts |> tryHead 0 |> getValueOrDefault 0
        Minor = intParts |> tryHead 1  |> getValueOrDefault 0
        Patch = intParts |> tryHead 2  |> getValueOrDefault 0
        SubPatch = intParts |> tryHead 3 |> getValueOrDefault 0
        PrereleaseId = parts |> tryHead 4 |> getValueOrDefault suffix
    }
