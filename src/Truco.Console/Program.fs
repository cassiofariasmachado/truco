open System
open Truco.Core.Models
open Truco.Core.GameIO
open Truco.Core.GameLoop
open Truco.Console.ConsoleGameIO

[<EntryPoint>]
let main _ =

    printfn "╔══════════════════════════════════╗"
    printfn "║     Welcome to Truco Game!       ║"
    printfn "╚══════════════════════════════════╝"
    printfn ""

    // Create console IO using functional GameIO record pattern
    let io = create()

    printfn "Enter Player One's name:"

    let playerOneName =
        match Console.ReadLine() with
        | name when String.IsNullOrWhiteSpace(name) -> "Player One"
        | name -> name

    printfn "Enter Player Two's name:"

    let playerTwoName =
        match Console.ReadLine() with
        | name when String.IsNullOrWhiteSpace(name) -> "Player Two"
        | name -> name

    printfn ""
    printfn "Match will be played to 12 points"
    printfn "Each round consists of up to 3 turns (best of 3)"
    printfn ""

    let playerOne = createPlayer playerOneName []
    let playerTwo = createPlayer playerTwoName []

    let rng = Random()
    let initialMatch = createMatch playerOne playerTwo
    let finalMatch = playMatch io rng initialMatch 12

    io.ShowMatchResult finalMatch

    printfn "\nPress any key to exit..."
    Console.ReadKey() |> ignore

    0
