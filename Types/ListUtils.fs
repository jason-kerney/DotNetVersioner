[<AutoOpen>]
module Version.DotNet.Types.ListUtils

let rec trySkip count = function
    | [] -> []
    | l when count <= 0  ->
        l
    | _::tail -> tail |> trySkip (count - 1)
    
let rec tryHead count items =
    let next = items |> trySkip count
    next
    |> List.tryHead
    
    