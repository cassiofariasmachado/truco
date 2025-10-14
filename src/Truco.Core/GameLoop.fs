namespace Truco.Core

open Truco.Core.Models
open Truco.Core.Actions

module GameLoop =

    // Determine if a player has won the round (2 out of 3 turns)
    let determineRoundWinner (round: Round) : RoundWinner option =
        match round.TurnHistory with
        | [] -> None
        | turns ->
            let turnWinners = 
                turns 
                |> List.rev
                |> List.choose (fun turn -> turn.Winner)
            
            match turnWinners with
            | [] -> None
            | _ ->
                let firstTurn = turns.Head
                let playerOneName = firstTurn.PlayersMoveOne.Player.Name
                let playerTwoName = firstTurn.PlayersMoveTwo.Player.Name
                
                let playerOneWins = turnWinners |> List.filter (fun p -> p.Name = playerOneName) |> List.length
                let playerTwoWins = turnWinners |> List.filter (fun p -> p.Name = playerTwoName) |> List.length
                
                if playerOneWins >= 2 then
                    Some (RoundWinner firstTurn.PlayersMoveOne.Player)
                elif playerTwoWins >= 2 then
                    Some (RoundWinner firstTurn.PlayersMoveTwo.Player)
                else
                    None

    // Award points to the round winner
    let awardRoundPoints (_match: Match) (roundWinner: RoundWinner) : Match =
        let (RoundWinner winner) = roundWinner
        
        if winner.Name = _match.PlayerOne.Player.Name then
            { _match with PlayerOne = addPoint _match.PlayerOne }
        else
            { _match with PlayerTwo = addPoint _match.PlayerTwo }

    // Deal new hands to both players
    let dealNewHands (_match: Match) : Match =
        let deck = shuffleDeck()
        
        let handPlayerOne = [ deck.[0]; deck.[2]; deck.[4] ]
        let handPlayerTwo = [ deck.[1]; deck.[3]; deck.[5] ]
        
        let playerOne = { _match.PlayerOne.Player with Hand = handPlayerOne }
        let playerTwo = { _match.PlayerTwo.Player with Hand = handPlayerTwo }
        
        { _match with 
            PlayerOne = { _match.PlayerOne with Player = playerOne }
            PlayerTwo = { _match.PlayerTwo with Player = playerTwo } }

    // Check if a player has won the match (reached target score)
    let checkMatchWinner (_match: Match) (targetScore: int) : MatchWinner option =
        if _match.PlayerOne.Points >= targetScore then
            Some (MatchWinner _match.PlayerOne.Player)
        elif _match.PlayerTwo.Points >= targetScore then
            Some (MatchWinner _match.PlayerTwo.Player)
        else
            None

    // Play a single turn
    let playTurn (_match: Match) (cardPlayerOne: Card) (cardPlayerTwo: Card) : Match =
        turn _match cardPlayerOne cardPlayerTwo

    // Get the player who should play first in the turn
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

    // Play a complete round (up to 3 turns)
    let rec playRound (_match: Match) (getPlayerChoice: Player -> Card) : Match =
        let currentRound = _match.RoundHistory.Head
        
        // Check if round is complete (someone won 2 turns or all 3 turns played)
        let roundWinner = determineRoundWinner currentRound
        
        match roundWinner with
        | Some winner ->
            // Round is complete, award points
            let updatedRound = { currentRound with Winner = Some winner }
            let updatedMatch = { _match with RoundHistory = updatedRound :: _match.RoundHistory.Tail }
            awardRoundPoints updatedMatch winner
        | None when currentRound.TurnHistory.Length >= 3 ->
            // All 3 turns played, it's a draw - no points awarded
            _match
        | None ->
            // Play another turn
            let cardPlayerOne = getPlayerChoice _match.PlayerOne.Player
            let cardPlayerTwo = getPlayerChoice _match.PlayerTwo.Player
            
            let updatedMatch = playTurn _match cardPlayerOne cardPlayerTwo
            playRound updatedMatch getPlayerChoice

    // Start a new round
    let startNewRound (_match: Match) : Match =
        let newRound = createRound
        let matchWithNewRound = { _match with RoundHistory = newRound :: _match.RoundHistory }
        dealNewHands matchWithNewRound

    // Play a complete match (until someone reaches target score)
    let rec playMatch (_match: Match) (targetScore: int) (getPlayerChoice: Player -> Card) : Match =
        let matchWinner = checkMatchWinner _match targetScore
        
        match matchWinner with
        | Some winner ->
            // Match is complete
            { _match with Winner = Some winner }
        | None ->
            // Start a new round
            let matchWithNewRound = startNewRound _match
            let matchAfterRound = playRound matchWithNewRound getPlayerChoice
            playMatch matchAfterRound targetScore getPlayerChoice
