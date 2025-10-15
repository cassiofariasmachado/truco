namespace Truco.Core

open Truco.Core.Models
open MoreLinq

module Actions =

    /// <summary>
    ///   Gets the card value based
    /// </summary>
    let cardValue card = cardValues.Item card

    /// <summary>
    ///   Determines if a card is black
    /// </summary>
    let isBlackCard card =
        let value = cardValue card

        match value with
        | 14
        | 13
        | 12
        | 11 -> false
        | _ -> true

    /// <summary>
    ///   Shuffles the deck of cards
    /// </summary>
    let shuffleDeck () : Deck =
        MoreEnumerable.RandomSubset(cardValues.Keys, cardValues.Keys.Count)
        |> List.ofSeq

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
        let updatedLastRound = addTurnToTurnHistory _match.RoundHistory.Head turn

        { _match with
            RoundHistory = updatedLastRound :: _match.RoundHistory }

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
