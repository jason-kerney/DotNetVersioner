module Utils.Maybe.MaybeSeq

let reduceErrors (items: _ maybe seq) =
    let maybeFailures (items: _ list) =
        if 0 < items.Length then items |> asCombinationFailure
        else Ok ()
        
    items
    |> Seq.toList
    |> List.filter Maybe.isError
    |> List.map (fun (Error e) -> e)
    |> maybeFailures