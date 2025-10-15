namespace Truco.Core

open Xunit
open FsUnit
open Truco.Core.Models

module ModelsTest =

    [<Fact>]
    let ``Should create a hand correctly`` () =
        let cards = [ (Ace, Sword); (Ace, Club); (Two, Cup) ]

        let hand = createHand cards

        hand |> should equal (cards)

    [<Fact>]
    let ``Should create a hand with an empty list`` () =
        let hand = createHand []

        hand |> should equivalent []

    [<Fact>]
    let ``Should create a player correctly`` () =
        let name = "John"
        let hand = createHand [ (Ace, Sword) ]

        let player = createPlayer name hand

        player.Name |> should equal name
        player.Hand |> should equal hand

    [<Fact>]
    let ``Should create a player move correctly`` () =
        let player = createPlayer "John" []
        let card = (Ace, Sword)

        let playerMove = createPlayerMove player card

        playerMove.Player |> should equal player
        playerMove.PlayedCard |> should equal card

    [<Fact>]
    let ``Should create a turn correctly`` () =
        let playerOne = createPlayer "John" []
        let cardPlayerOne = (Two, Cup)
        let playerTwo = createPlayer "Mary" []
        let cardPlayerTwo = (Ace, Sword)

        let playerOneMove = createPlayerMove playerOne cardPlayerOne
        let playerTwoMove = createPlayerMove playerTwo cardPlayerTwo

        let turn = createTurn playerOneMove playerTwoMove (Some playerOne)

        turn.PlayersMoveOne |> should equal playerOneMove
        turn.PlayersMoveTwo |> should equal playerTwoMove

    [<Fact>]
    let ``Should create a match player correctly`` () =
        let player = createPlayer "John" [ (Ace, Sword) ]
        let matchPlayer = createMatchPlayer player

        matchPlayer.Player |> should equal player
        matchPlayer.Points |> should equal 0

    [<Fact>]
    let ``Should create a match correctly`` () =
        let playerOne = createPlayer "John" [ (Ace, Sword) ]
        let playerTwo = createPlayer "Mary" [ (Ace, Cup) ]

        let _match = createMatch playerOne playerTwo

        _match.PlayerOne.Player |> should equal playerOne
        _match.PlayerOne.Points |> should equal 0
        _match.PlayerTwo.Player |> should equal playerTwo
        _match.PlayerTwo.Points |> should equal 0
        _match.RoundHistory |> should be Empty
        _match.Winner |> should equal None
