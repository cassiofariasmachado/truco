namespace Truco.Console

open System
open Truco.Core.Models
open Truco.Core.GameIO

module ConsoleGameIO =

    type ConsoleGameIO() =
        interface IGameIO with
            member _.ShowMessage(msg: string) = printfn $"{msg}"

            member _.ShowHand(player: Player) =
                printfn ""
                printfn $"{player.Name}'s Hand:"

                player.Hand
                |> List.iteri (fun i card ->
                    let (rank, suit) = card
                    printfn $"  [{i + 1}] {rank} of {suit}")

            member _.GetCardChoice(player: Player) =
                printfn ""
                printfn $"{player.Name}, choose a card to play (1-{player.Hand.Length}): "

                let rec getValidInput () =
                    match Console.ReadLine() with
                    | input when String.IsNullOrWhiteSpace(input) ->
                        printfn "Please enter a number:"
                        getValidInput ()
                    | input ->
                        match Int32.TryParse(input) with
                        | (true, num) when num >= 1 && num <= player.Hand.Length -> num - 1
                        | _ ->
                            printfn $"Invalid choice. Please enter a number between 1 and {player.Hand.Length}:"
                            getValidInput ()

                getValidInput ()

            member _.ShowTurnResult(turn: Turn) =
                let (rankOne, suitOne) = turn.PlayersMoveOne.PlayedCard
                let (rankTwo, suitTwo) = turn.PlayersMoveTwo.PlayedCard

                printfn ""
                printfn "--- Turn Result ---"
                printfn $"{turn.PlayersMoveOne.Player.Name} played: {rankOne} of {suitOne}"
                printfn $"{turn.PlayersMoveTwo.Player.Name} played: {rankTwo} of {suitTwo}"

                match turn.Winner with
                | Some winner -> printfn $"Winner: {winner.Name}"
                | None -> printfn "Result: Draw"

            member _.ShowRoundResult (round: Round) (playerOne: MatchPlayer) (playerTwo: MatchPlayer) =
                printfn ""
                printfn "=== Round Complete ==="

                match round.Winner with
                | Some(RoundWinner winner) -> printfn $"Round Winner: {winner.Name}"
                | None -> printfn "Round ended in a draw"

                printfn $"Turns played: {round.TurnHistory.Length}"

            member _.ShowScore (playerOne: MatchPlayer) (playerTwo: MatchPlayer) =
                printfn ""
                printfn "--- Current Score ---"
                printfn $"{playerOne.Player.Name}: {playerOne.Points} points"
                printfn $"{playerTwo.Player.Name}: {playerTwo.Points} points"
                printfn "--------------------"

            member _.ShowMatchResult(_match: Match) =
                printfn ""
                printfn "╔══════════════════════════════════╗"
                printfn "║     MATCH COMPLETE!              ║"
                printfn "╚══════════════════════════════════╝"

                match _match.Winner with
                | Some(MatchWinner winner) ->
                    printfn ""
                    printfn $"🏆 Winner: {winner.Name} 🏆"
                | None -> printfn "\nMatch ended in a draw"

                printfn ""
                printfn "Final Score:"
                printfn $"  {_match.PlayerOne.Player.Name}: {_match.PlayerOne.Points} points"
                printfn $"  {_match.PlayerTwo.Player.Name}: {_match.PlayerTwo.Points} points"
                printfn ""
                printfn $"Rounds played: {_match.RoundHistory.Length}"
