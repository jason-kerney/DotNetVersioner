[<AutoOpen>]
module Utils.Maybe.Operators

let andM (value: _ maybe) (check: unit maybe) =
    match check with
    | Ok () -> value
    | Error e -> Error e
    
let inline (&!) a b = a |> andM b
let (/->) a f = a |> apply f
let (<-/) f a = a /-> f

let (|->) a (f: Maybe<_ maybe -> _ maybe>) =
    match f with
    | Ok fn -> fn a
    | Error e -> Error e
    
let (<-|) f a = a |-> f