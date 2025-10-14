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

    let io = ConsoleGameIO() :> IGameIO

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

    let initialMatch = createMatch playerOne playerTwo

    let getPlayerChoice (player: Player) : Card =
        io.ShowHand(player)
        let cardIndex = io.GetCardChoice(player)
        player.Hand[cardIndex]

    let rec playRoundWithIO (_match: Match) : Match =
        let currentRound = _match.RoundHistory.Head

        let roundWinner = determineRoundWinner currentRound

        match roundWinner with
        | Some winner ->
            let updatedRound =
                { currentRound with
                    Winner = Some winner }

            let updatedMatch =
                { _match with
                    RoundHistory = updatedRound :: _match.RoundHistory.Tail }

            let finalMatch = awardRoundPoints updatedMatch winner

            io.ShowRoundResult updatedRound finalMatch.PlayerOne finalMatch.PlayerTwo

            io.ShowScore finalMatch.PlayerOne finalMatch.PlayerTwo

            finalMatch
        | None when currentRound.TurnHistory.Length >= 3 ->
            io.ShowRoundResult currentRound _match.PlayerOne _match.PlayerTwo
            io.ShowScore _match.PlayerOne _match.PlayerTwo

            _match
        | None ->
            let turnNumber = currentRound.TurnHistory.Length + 1

            io.ShowMessage $"\n========== TURN {turnNumber:d2} =========="

            let cardPlayerOne = getPlayerChoice _match.PlayerOne.Player
            let cardPlayerTwo = getPlayerChoice _match.PlayerTwo.Player

            let updatedMatch = playTurn _match cardPlayerOne cardPlayerTwo

            let lastTurn = updatedMatch.RoundHistory.Head.TurnHistory.Head
            
            io.ShowTurnResult(lastTurn)

            playRoundWithIO updatedMatch

    let rec playMatchWithIO (_match: Match) (targetScore: int) : Match =
        let matchWinner = checkMatchWinner _match targetScore

        match matchWinner with
        | Some winner -> { _match with Winner = Some winner }
        | None ->
            let completedRounds =
                _match.RoundHistory |> List.filter (fun r -> r.Winner.IsSome) |> List.length

            let roundNumber = completedRounds + 1

            io.ShowMessage("\n\n╔══════════════════════════════════╗")
            io.ShowMessage($"║       ROUND {roundNumber:d2}                   ║")
            io.ShowMessage("╚══════════════════════════════════╝")

            let matchWithNewRound = startNewRound _match
            let matchAfterRound = playRoundWithIO matchWithNewRound

            playMatchWithIO matchAfterRound targetScore

    let finalMatch = playMatchWithIO initialMatch 12

    io.ShowMatchResult(finalMatch)

    printfn "\nPress any key to exit..."
    Console.ReadKey() |> ignore

    0
