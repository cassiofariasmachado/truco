namespace Truco.Core

open System
open Truco.Core.Models
open MoreLinq


module Actions =

    let cardValue card = cardValues.Item card

    let isBlackCard card =
        let value = cardValue card

        match value with
        | 14
        | 13
        | 12
        | 11 -> false
        | _ -> true

    let shuffleDeck(): Deck = MoreEnumerable.RandomSubset(cardValues.Keys, cardValues.Keys.Count) |> List.ofSeq

    let checkTurnWinner (playerOne: Player, cardOne) (playerTwo: Player, cardTwo) =
        match (cardValue cardOne, cardValue cardTwo) with
        | (valueOne, valueTwo) when valueOne > valueTwo -> Some(playerOne)
        | (valueOne, valueTwo) when valueOne < valueTwo -> Some(playerTwo)
        | _ -> None

    let removeCard player card =
        let hand = player.Hand |> List.filter ((<>) card)
        { player with Hand = hand }

    let addTurnToTurnHistory round turn = { round with TurnHistory = turn :: round.TurnHistory }

    let addTurnToRoundHistory _match turn =
        let updatedLastRound = addTurnToTurnHistory _match.RoundHistory.Head turn
        { _match with RoundHistory = updatedLastRound :: _match.RoundHistory }

    let addPoint player = { player with Points = player.Points + 1 }

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
