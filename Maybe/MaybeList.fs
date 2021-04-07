module Utils.Maybe.MaybeList

let reduceErrors = MaybeSeq.reduceErrors

let concat (items : _ list mlist) : _ mlist=
    maybe {
        let! items = items
        
        return items |> List.concat
    }
    
let exists predicate (items: _ mlist) =
    maybe {
        let! items = items
        return
            items
            |> List.exists predicate
    } |> Maybe.toBool
    
let find predicate : _ mlist -> _ maybe =
    predicate
    |> List.find
    |> Maybe.lift

let filter predicate : _ mlist -> _ mlist =
    predicate
    |> List.filter
    |> Maybe.lift
    
let sort (list: _ mlist) : _ mlist =
    list |> Maybe.lift List.sort
    
let sortBy f (list: _ mlist) : _ mlist =
    f
    |> Ok
    |> Maybe.lift List.sortBy
        <-/ list

let flatten (items: _ maybe list) : _ mlist =
    maybe {
        let errors =
            items
            |> List.filter (fun item -> match item with Error _ -> true | _ -> false)
            |> List.map (fun (Error item) -> item)
        
        if 0 < errors.Length
        then return! errors |> asCombinationFailure
        else
            return
                items
                |> List.map (fun (Ok item) -> item)
    }
    
let map f : _ mlist -> _ mlist = f |> List.map |> Maybe.lift

let mapM f items : _ mlist =
    maybe {
        let! items =
            items |> map (fun a -> a |> asMaybe |> f)
            
        return! items |> flatten
    }
    
let cons (items: _ mlist) item : _ mlist =
    maybe {
        let! items = items
        return item::items
    }

let consM items item : _ mlist =
    maybe {
        let! item = item
        return! item |> cons items
    }

let head (list: _ mlist) = (List.head |> Maybe.lift) list

let tail (list: _ mlist) = (List.tail |> Maybe.lift) list

let toListM item : _ mlist =
    maybe {
        let! item = item
        return item::[]
    }
    
let iter f (items: _ mlist) =
    maybe {
        let! items = items
        
        return
            items
            |> List.iter f
    }

let iterM (f: _ maybe -> unit maybe) (items: _ mlist) : unit maybe =
    let result = items |> mapM f
    
    match result with
    | Ok _ -> Ok ()
    | Error e -> Error e

let iter_M (f: _ -> unit maybe) (items: _ mlist) : unit maybe =
    maybe {
        let! items = items
        
        return!
            items
            |> List.map f
            |> reduceErrors
    }
    
let append (itemsA: _ mlist) (itemsB: _ mlist) : _ mlist =
    maybe {
        let! itemsA = itemsA
        let! itemsB = itemsB
        
        return
            itemsB
            |> List.append itemsA
    }

let except (itemsA: _ mlist) (itemsB: _ mlist) : _ mlist =
    maybe {
        let! itemsA = itemsA
        let! itemsB = itemsB
        
        let result =
            itemsB
            |> List.except itemsA
        
        return result
    }
    
let contains item (items: _ mlist) =
    maybe {
        let! items = items
        return
            items
            |> List.contains item
    } |> Maybe.toBool
    
let simplify (items: _ maybe mlist) : _ mlist =
    maybe {
        let! items = items
        return! items |> flatten
    }
    
let reduce f (items: _ mlist): _ maybe =
    maybe {
        let! items = items
        
        return items |> List.reduce f
    }