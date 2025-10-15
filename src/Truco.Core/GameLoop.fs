namespace Truco.Core

open System
open Truco.Core.Models
open Truco.Core.Actions
open Truco.Core.GameIO

module GameLoop =

    /// <summary>
    ///   Helper to compare players structurally
    /// </summary>
    let private playersEqual (p1: Player) (p2: Player) =
        p1.Name = p2.Name

    /// <summary>
    ///   Determine if a player has won the round.
    ///   <remarks>
    ///     The player wins the round if they win 2 out of 3 turns, or in case of a draw, the winner is whoever won the first turn.
    ///   </remarks>
    /// </summary>
    let determineRoundWinner (round: Round) : RoundWinner option =
        match round.TurnHistory with
        | [] -> None
        | firstTurn :: _ as turns ->
            let turnWinners = turns |> List.rev |> List.choose (fun turn -> turn.Winner)

            match turnWinners with
            | [] -> None
            | _ ->
                let playerOne = firstTurn.PlayersMoveOne.Player
                let playerTwo = firstTurn.PlayersMoveTwo.Player

                let playerOneWins =
                    turnWinners |> List.filter (playersEqual playerOne) |> List.length

                let playerTwoWins =
                    turnWinners |> List.filter (playersEqual playerTwo) |> List.length

                if playerOneWins >= 2 then
                    Some(RoundWinner playerOne)
                elif playerTwoWins >= 2 then
                    Some(RoundWinner playerTwo)
                else
                    None

    /// <summary>
    /// Award points to the round winner
    /// </summary>
    let awardRoundPoints (_match: Match) (roundWinner: RoundWinner) : Match =
        let (RoundWinner winner) = roundWinner

        if playersEqual winner _match.PlayerOne.Player then
            { _match with
                PlayerOne = addPoint _match.PlayerOne }
        else
            { _match with
                PlayerTwo = addPoint _match.PlayerTwo }

    /// <summary>
    /// Deal new hands to both players
    /// </summary>
    let dealNewHands (rng: Random) (_match: Match) : Match =
        let deck = shuffleDeck rng

        match deck with
        | c1 :: c2 :: c3 :: c4 :: c5 :: c6 :: _ ->
            let handPlayerOne = [ c1; c3; c5 ]
            let handPlayerTwo = [ c2; c4; c6 ]

            let playerOne =
                { _match.PlayerOne.Player with
                    Hand = handPlayerOne }

            let playerTwo =
                { _match.PlayerTwo.Player with
                    Hand = handPlayerTwo }

            { _match with
                PlayerOne.Player = playerOne
                PlayerTwo.Player = playerTwo }
        | _ -> 
            // This should never happen with a proper deck, but handle gracefully
            _match

    /// <summary>
    ///   Check if a player has won the match (reached target score)
    /// </summary>
    let checkMatchWinner (_match: Match) (targetScore: int) : MatchWinner option =
        if _match.PlayerOne.Points >= targetScore then
            Some(MatchWinner _match.PlayerOne.Player)
        elif _match.PlayerTwo.Points >= targetScore then
            Some(MatchWinner _match.PlayerTwo.Player)
        else
            None

    /// <summary>
    ///   Play a single turn
    /// </summary>
    let playTurn (_match: Match) (cardPlayerOne: Card) (cardPlayerTwo: Card) : Match =
        turn _match cardPlayerOne cardPlayerTwo

    /// <summary>
    ///   Get the player who should play first in the turn
    /// </summary>
    let getFirstPlayer (_match: Match) : Player =
        match _match.RoundHistory with
        | [] -> _match.PlayerOne.Player
        | currentRound :: _ ->
            match currentRound.TurnHistory with
            | [] -> _match.PlayerOne.Player
            | lastTurn :: _ ->
                lastTurn.Winner |> Option.defaultValue _match.PlayerOne.Player

    /// <summary>
    ///   Start a new round
    /// </summary>
    let startNewRound (rng: Random) (_match: Match) : Match =
        let newRound = createRound

        let matchWithNewRound =
            { _match with
                RoundHistory = newRound :: _match.RoundHistory }

        dealNewHands rng matchWithNewRound

    /// <summary>
    ///   Get the player's card choice via GameIO
    /// </summary>
    let getPlayerChoice (io: GameIO) (player: Player) : Card =
        io.ShowHand player
        let cardIndex = io.GetCardChoice player
        player.Hand[cardIndex]

    /// <summary>
    ///   Play a single round until a player wins or maximum turns are reached
    /// </summary>
    let rec playRound (io: GameIO) (_match: Match) : Match =
        match _match.RoundHistory with
        | [] -> _match // No round to play
        | currentRound :: rest ->
            let roundWinner = determineRoundWinner currentRound
            let getPlayerChoiceWithIO = getPlayerChoice io

            match roundWinner with
            | Some winner ->
                let updatedRound =
                    { currentRound with
                        Winner = Some winner }

                let updatedMatch =
                    { _match with
                        RoundHistory = updatedRound :: rest }

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

                let cardPlayerOne = getPlayerChoiceWithIO _match.PlayerOne.Player
                let cardPlayerTwo = getPlayerChoiceWithIO _match.PlayerTwo.Player

                let updatedMatch = playTurn _match cardPlayerOne cardPlayerTwo

                match updatedMatch.RoundHistory with
                | currentRound :: _ ->
                    match currentRound.TurnHistory with
                    | lastTurn :: _ ->
                        io.ShowTurnResult lastTurn
                        playRound io updatedMatch
                    | [] -> 
                        playRound io updatedMatch
                | [] -> 
                    updatedMatch

    /// <summary>
    ///   Play the match until a player reaches the target score
    /// </summary>
    let rec playMatch (io: GameIO) (rng: Random) (_match: Match) (targetScore: int) : Match =
        let matchWinner = checkMatchWinner _match targetScore
        let playRoundWithIO = playRound io

        match matchWinner with
        | Some winner -> { _match with Winner = Some winner }
        | None ->
            let completedRounds =
                _match.RoundHistory |> List.filter (fun r -> r.Winner.IsSome) |> List.length

            let roundNumber = completedRounds + 1

            io.ShowMessage "\n\n╔══════════════════════════════════╗"
            io.ShowMessage $"║       ROUND {roundNumber:d2}                   ║"
            io.ShowMessage "╚══════════════════════════════════╝"

            let matchWithNewRound = startNewRound rng _match
            let matchAfterRound = playRoundWithIO matchWithNewRound

            playMatch io rng matchAfterRound targetScore
