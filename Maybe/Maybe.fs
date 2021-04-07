module Utils.Maybe.Maybe

let wrap : _ -> _ maybe = Ok
    
let orMaybe (a: _ maybe) (b: _ maybe) : _ maybe =
    match b, a with
    | Ok v, _
    | _, Ok v -> Ok v
    | _ -> b
    
let isPreferredOver (a: _ maybe) (b: _ maybe) : _ maybe =
    b |> orMaybe a
    
let butPrefer (a: _ maybe) (b: _ maybe) : _ maybe =
    a |> orMaybe b

let simplify (value: _ maybe maybe) =
    maybe {
        let! value = value
        return! value
    }

let toBool (value: bool maybe) =
    match value with
    | Ok v -> v
    | Error _ -> false
    
let hasValue (possible: _ maybe) =
    match possible with
    | Ok _ -> true
    | _ -> false
    
let isError (value: _ maybe) =
    match value with
    | Ok _ -> false
    | _ -> true
    
let orDefault defaultValue (value: _ maybe) =
    match value with
    | Ok v -> v
    | _ -> defaultValue
    
let lift f : _ maybe -> _ maybe = Result.map f
let mlift (f: _ -> _ maybe) : _ maybe -> _ maybe = Result.bind f

let llift value f = (lift f) value

let mllift value f = (mlift f) value

let partialLift (f: _ -> _ maybe) : _ maybe -> _ maybe = Result.bind f

let map f (item: _ maybe) =
    maybe {
        let! item = item
        
        return
            f item
    }
    
let check f item =
    item
    |> map f
    |> toBool
    
let fromOption errorMsg : _ option -> _ maybe = function
    | Some v -> Ok v
    | _ -> errorMsg |> asGeneralFailure
    
let callToString : _ maybe -> string = function
    | Ok v -> v.ToString ()
    | Error e -> e |> Error |> sprintf "%A"