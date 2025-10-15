namespace Truco.Core

open System
open Truco.Core.Models

module Actions =

    /// <summary>
    ///   Gets the card value based on the card
    /// </summary>
    let cardValue card = 
        Map.find card cardValues

    /// <summary>
    ///   Determines if a card is black (lower value cards)
    /// </summary>
    let isBlackCard card =
        let value = cardValue card
        value < fourthHighestCardValue

    /// <summary>
    ///   Fisher-Yates shuffle algorithm
    /// </summary>
    let private shuffleArray (rng: Random) (array: 'a[]) =
        let swap i j =
            let temp = array.[i]
            array.[i] <- array.[j]
            array.[j] <- temp
        
        for i in (Array.length array - 1) .. -1 .. 1 do
            let j = rng.Next(0, i + 1)
            swap i j
        array

    /// <summary>
    ///   Shuffles the deck of cards using Fisher-Yates algorithm
    /// </summary>
    let shuffleDeck (rng: Random) : Deck =
        cardValues
        |> Map.toArray
        |> Array.map fst
        |> shuffleArray rng
        |> Array.toList

    /// <summary>
    ///   Checks which player won the turn
    /// </summary>
    let checkTurnWinner (playerOne: Player, cardOne) (playerTwo: Player, cardTwo) =
        match (cardValue cardOne, cardValue cardTwo) with
        | valueOne, valueTwo when valueOne > valueTwo -> Some(playerOne)
        | valueOne, valueTwo when valueOne < valueTwo -> Some(playerTwo)
        | _ -> None

    /// <summary>
    ///   Removes a card from a player's hand
    /// </summary>
    let removeCard player card =
        let hand = player.Hand |> List.filter ((<>) card)
        { player with Hand = hand }

    /// <summary>
    ///   Adds a turn to the turn history of a round
    /// </summary>
    let addTurnToTurnHistory round turn =
        { round with
            TurnHistory = turn :: round.TurnHistory }

    /// <summary>
    ///   Adds a turn to the round history of a match
    /// </summary>
    let addTurnToRoundHistory _match turn =
        match _match.RoundHistory with
        | [] -> _match // No round to add turn to
        | lastRound :: rest ->
            let updatedLastRound = addTurnToTurnHistory lastRound turn
            { _match with
                RoundHistory = updatedLastRound :: rest }

    /// <summary>
    ///   Adds a point to a match player
    /// </summary>
    let addPoint player =
        { player with
            Points = player.Points + 1 }

    /// <summary>
    ///   Plays a turn for both players, updating their hands and the match state
    /// </summary>
    let turn _match playerOneCard playerTwoCard =
        let playerOne = removeCard _match.PlayerOne.Player playerOneCard
        let playerTwo = removeCard _match.PlayerTwo.Player playerTwoCard

        let winner = checkTurnWinner (playerOne, playerOneCard) (playerTwo, playerTwoCard)

        let playerOneMove = createPlayerMove playerOne playerOneCard
        let playerTwoMove = createPlayerMove playerTwo playerTwoCard
        let turn = createTurn playerOneMove playerTwoMove winner

        let updatedMatchWithTurn = addTurnToRoundHistory _match turn

        { updatedMatchWithTurn with
            PlayerOne.Player = playerOne
            PlayerTwo.Player = playerTwo }
