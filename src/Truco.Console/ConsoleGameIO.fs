namespace Truco.Console

open System
open Truco.Core.Models
open Truco.Core.GameIO

module ConsoleGameIO =

    /// <summary>
    ///   Pure function to show a message
    /// </summary>
    let private showMessage (msg: string) = 
        printfn $"{msg}"

    /// <summary>
    ///   Pure function to display a player's hand
    /// </summary>
    let private showHand (player: Player) =
        printfn ""
        printfn $"{player.Name}'s Hand:"
        
        player.Hand
        |> List.iteri (fun i (rank, suit) ->
            printfn $"  [{i + 1}] {rank} of {suit}")

    /// <summary>
    ///   Get card choice from player
    /// </summary>
    let private getCardChoice (player: Player) =
        printfn ""
        printfn $"{player.Name}, choose a card to play (1-{player.Hand.Length}): "
        
        let rec getValidInput () =
            match Console.ReadLine() with
            | input when String.IsNullOrWhiteSpace(input) ->
                printfn "Please enter a number:"
                getValidInput ()
            | input ->
                match Int32.TryParse(input) with
                | (true, num) when num >= 1 && num <= player.Hand.Length -> 
                    num - 1
                | _ ->
                    printfn $"Invalid choice. Please enter a number between 1 and {player.Hand.Length}:"
                    getValidInput ()
        
        getValidInput ()

    /// <summary>
    ///   Display turn result
    /// </summary>
    let private showTurnResult (turn: Turn) =
        let (rankOne, suitOne) = turn.PlayersMoveOne.PlayedCard
        let (rankTwo, suitTwo) = turn.PlayersMoveTwo.PlayedCard

        printfn ""
        printfn "--- Turn Result ---"
        printfn $"{turn.PlayersMoveOne.Player.Name} played: {rankOne} of {suitOne}"
        printfn $"{turn.PlayersMoveTwo.Player.Name} played: {rankTwo} of {suitTwo}"

        match turn.Winner with
        | Some winner -> printfn $"Winner: {winner.Name}"
        | None -> printfn "Result: Draw"

    /// <summary>
    ///   Display round result
    /// </summary>
    let private showRoundResult (round: Round) (playerOne: MatchPlayer) (playerTwo: MatchPlayer) =
        printfn ""
        printfn "=== Round Complete ==="

        match round.Winner with
        | Some(RoundWinner winner) -> printfn $"Round Winner: {winner.Name}"
        | None -> printfn "Round ended in a draw"

        printfn $"Turns played: {round.TurnHistory.Length}"

    /// <summary>
    ///   Display current score
    /// </summary>
    let private showScore (playerOne: MatchPlayer) (playerTwo: MatchPlayer) =
        printfn ""
        printfn "--- Current Score ---"
        printfn $"{playerOne.Player.Name}: {playerOne.Points} points"
        printfn $"{playerTwo.Player.Name}: {playerTwo.Points} points"
        printfn "--------------------"

    /// <summary>
    ///   Display match result
    /// </summary>
    let private showMatchResult (_match: Match) =
        printfn ""
        printfn "╔══════════════════════════════════╗"
        printfn "║     MATCH COMPLETE!              ║"
        printfn "╚══════════════════════════════════╝"

        match _match.Winner with
        | Some(MatchWinner winner) ->
            printfn ""
            printfn $"🏆 Winner: {winner.Name} 🏆"
        | None -> 
            printfn "\nMatch ended in a draw"

        printfn ""
        printfn "Final Score:"
        printfn $"  {_match.PlayerOne.Player.Name}: {_match.PlayerOne.Points} points"
        printfn $"  {_match.PlayerTwo.Player.Name}: {_match.PlayerTwo.Points} points"
        printfn ""
        printfn $"Rounds played: {_match.RoundHistory.Length}"

    /// <summary>
    ///   Create a console-based GameIO record (functional pattern)
    /// </summary>
    let create () : GameIO = {
        ShowMessage = showMessage
        ShowHand = showHand
        GetCardChoice = getCardChoice
        ShowTurnResult = showTurnResult
        ShowRoundResult = showRoundResult
        ShowMatchResult = showMatchResult
        ShowScore = showScore
    }
