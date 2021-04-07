[<AutoOpen>]
module Utils.Maybe.MaybeMonad

let asMaybe value: _ maybe = value |> Ok

let private delayMaybe f: _ maybe = f ()

let bindWith rest (maybe: _ maybe): _ maybe =
    try
        match maybe with
        | Ok value -> value |> rest
        | Error e -> Error e
    with
    | e -> e |> ExceptionFailure |> Error

let combineMaybeWith maybe2 (maybe1: _ maybe): _ maybe =
    try
        match maybe1 with
        | Ok _ -> maybe2
        | Error e -> Error e
    with
    | e -> e |> ExceptionFailure |> Error

let ready: _ maybe = () |> Ok
    
type MaybeBuilder () =
    member __.Return value: _ maybe = value |> asMaybe
    member __.ReturnFrom maybe: _ maybe = maybe
    member __.Delay f = f |> delayMaybe
    member __.Bind (maybe, rest) = maybe |> bindWith rest
    member __.Combine (maybe1, maybe2) = maybe1 |> combineMaybeWith maybe2
    member __.Zero () = ready

let maybe = MaybeBuilder ()