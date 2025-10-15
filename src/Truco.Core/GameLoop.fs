namespace Truco.Core

open Truco.Core.Models
open Truco.Core.Actions
open Truco.Core.GameIO

module GameLoop =

    /// <summary>
    ///   Determine if a player has won the round.
    ///   <remarks>
    ///     The player wins the round if they win 2 out of 3 turns, or in case of a draw, the winner is whoever won the first turn.
    ///   </remarks>
    /// </summary>
    let determineRoundWinner (round: Round) : RoundWinner option =
        match round.TurnHistory with
        | [] -> None
        | turns ->
            let turnWinners = turns |> List.rev |> List.choose (fun turn -> turn.Winner)

            match turnWinners with
            | [] -> None
            | _ ->
                let firstTurn = turns.Head
                let playerOneName = firstTurn.PlayersMoveOne.Player.Name
                let playerTwoName = firstTurn.PlayersMoveTwo.Player.Name

                let playerOneWins =
                    turnWinners |> List.filter (fun p -> p.Name = playerOneName) |> List.length

                let playerTwoWins =
                    turnWinners |> List.filter (fun p -> p.Name = playerTwoName) |> List.length

                if playerOneWins >= 2 then
                    Some(RoundWinner firstTurn.PlayersMoveOne.Player)
                elif playerTwoWins >= 2 then
                    Some(RoundWinner firstTurn.PlayersMoveTwo.Player)
                else
                    None

    /// <summary>
    /// Award points to the round winner
    /// </summary>
    let awardRoundPoints (_match: Match) (roundWinner: RoundWinner) : Match =
        let (RoundWinner winner) = roundWinner

        if winner.Name = _match.PlayerOne.Player.Name then
            { _match with
                PlayerOne = addPoint _match.PlayerOne }
        else
            { _match with
                PlayerTwo = addPoint _match.PlayerTwo }

    /// <summary>
    /// Deal new hands to both players
    /// </summary>
    let dealNewHands (_match: Match) : Match =
        let deck = shuffleDeck ()

        let handPlayerOne = [ deck[0]; deck[2]; deck[4] ]
        let handPlayerTwo = [ deck[1]; deck[3]; deck[5] ]

        let playerOne =
            { _match.PlayerOne.Player with
                Hand = handPlayerOne }

        let playerTwo =
            { _match.PlayerTwo.Player with
                Hand = handPlayerTwo }

        { _match with
            PlayerOne.Player = playerOne
            PlayerTwo.Player = playerTwo }

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
                match lastTurn.Winner with
                | Some winner -> winner
                | None -> _match.PlayerOne.Player

    /// <summary>
    ///   Start a new round
    /// </summary>
    let startNewRound (_match: Match) : Match =
        let newRound = createRound

        let matchWithNewRound =
            { _match with
                RoundHistory = newRound :: _match.RoundHistory }

        dealNewHands matchWithNewRound

    /// <summary>
    ///   Get the player's card choice via IGameIO
    /// </summary>
    let getPlayerChoice (io: IGameIO) (player: Player) : Card =
        io.ShowHand(player)
        let cardIndex = io.GetCardChoice(player)
        player.Hand[cardIndex]

    /// <summary>
    ///   Play a single round until a player wins or maximum turns are reached
    /// </summary>
    let rec playRound (io: IGameIO) (_match: Match) : Match =
        let currentRound = _match.RoundHistory.Head

        let roundWinner = determineRoundWinner currentRound
        let getPlayerChoiceWithIO = getPlayerChoice io

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

            let cardPlayerOne = getPlayerChoiceWithIO _match.PlayerOne.Player
            let cardPlayerTwo = getPlayerChoiceWithIO _match.PlayerTwo.Player

            let updatedMatch = playTurn _match cardPlayerOne cardPlayerTwo

            let lastTurn = updatedMatch.RoundHistory.Head.TurnHistory.Head

            io.ShowTurnResult(lastTurn)

            playRound io updatedMatch

    /// <summary>
    ///   Play the match until a player reaches the target score
    /// </summary>
    let rec playMatch (io: IGameIO) (_match: Match) (targetScore: int) : Match =
        let matchWinner = checkMatchWinner _match targetScore
        let playRoundWithIO = playRound io

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

            playMatch io matchAfterRound targetScore
