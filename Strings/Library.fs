[<AutoOpen>]
module Utils.Strings.Library
open System
open Utils.Maybe

let toMaybeString (value: _ maybe) : _ maybe =
    match value with
    | Ok v -> v |> sprintf "%A" |> Ok
    | Error e -> Error e
    
let isValidString (item: string maybe) =
    match item with
    | Ok v ->
        0 < v.Length
    | Error _ -> false
    

let splitBy (separators: char[]) (text: string) =
    text.Split (separators, StringSplitOptions.RemoveEmptyEntries)
    |> Array.toList
    
let msplitBy (separators: char []) = splitBy separators |> Maybe.lift

let split (text: string) =
    text |> splitBy [|'\r';'\n'|]
    
let msplit : _ maybe -> _ mlist = Maybe.lift split

let joinBys (separator: string) (items: string seq) =
    String.Join (separator, items)
    
let joinBy (separator: char) (items: string seq) =
    String.Join (separator, items)
    
let mjoinBy separator : _ mlist -> _ maybe = joinBy separator |> Maybe.lift

let mjoinByM separator items =
    maybe {
        let! separator = separator
        
        return!
            items
            |> mjoinBy separator
    }
    
let joinByString (separator: string) (items: string seq) =
    String.Join (separator, items)
    
let mjoinByString separator : _ maybe -> _ maybe = separator |> joinByString |> Maybe.lift

let mjoinByStringM separator items =
    maybe {
        let! separator = separator
        return!
            items
            |> mjoinByString separator
    }
    
let join (items: string list) =
    items |> joinBy '\n'
    
let mjoin (items: _ maybe) : _ maybe = items |> Maybe.lift join

let trim (value: string) = value.Trim ()

let mtrim : _ maybe -> _ maybe = trim |> Maybe.lift

let msprintf format = sprintf format |> Maybe.lift
