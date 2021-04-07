namespace Utils.Maybe

type ProcessFailure =
    | ExceptionFailure of exn
    | GeneralFailure of string
    | CombinationError of ProcessFailure list

type maybe<'Value> = Result<'Value, ProcessFailure>
type Maybe<'Value> = maybe<'Value>
type mlist<'Value> = maybe<'Value list>

type mseq<'Value> = maybe<'Value seq>

[<AutoOpen>]
module BaseHelpers =
    let asGeneralFailure failure: _ maybe = failure |> GeneralFailure |> Error
    let asExceptionFailure failure: _ maybe = failure |> ExceptionFailure |> Error
    let asCombinationFailure failures: _ maybe =
        failures |> CombinationError |> Error
    let asFailureCombinedWith failure2 failure1: _ maybe =
        let error =
            match failure1, failure2 with
            | CombinationError failures1, CombinationError failures2 ->
                failures1
                |> List.append failures2
                |> CombinationError
            | CombinationError failures, e ->
                e::failures |> CombinationError
            | e, CombinationError failures ->
                e::failures |> CombinationError
            | e1, e2 ->
                e1::e2::[] |> CombinationError
            
        error |> Error
        
    let combineWith failure1 failure2 =
        match failure1, failure2 with
        | CombinationError f1, CombinationError f2 ->
            List.concat [f1; f2] |> asCombinationFailure
        | CombinationError f1, f2 ->
            f2::f1 |> asCombinationFailure
        | f1, CombinationError f2 ->
            f1::f2 |> asCombinationFailure
        | f1, f2 ->
            [f1; f2] |> asCombinationFailure
            
    let apply (maybeF: _ maybe) (maybeX: _ maybe) : _ maybe = 
        match maybeF,maybeX with
        | Ok f, Ok x -> Ok (f x)
        | Error failure, Ok _ -> Error failure
        | Ok _, Error failure -> Error failure
        | Error failure1, Error failure2 ->
            failure1
            |> combineWith failure2