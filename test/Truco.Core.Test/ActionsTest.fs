namespace Truco.Core

open Xunit
open FsUnit
open Truco.Core.Actions
open Truco.Core.Models

module ActionsTest =

    [<Fact>]
    let ``Card value of Ace of Swords should be 14``() =
        let aceOfSwords = (Ace, Sword)
        cardValue aceOfSwords |> should equal 14

    [<Fact>]
    let ``Card value of Four Cup should be 1``() =
        let fourCup = (Four, Cup)
        cardValue fourCup |> should equal 1

    [<Fact>]
    let ``Four Cup is a black card``() =
        let fourCup = (Four, Cup)
        isBlackCard fourCup |> should be True

    [<Fact>]
    let ``Ace of Swords isn't a black card``() =
        let aceOfSwords = (Ace, Sword)
        isBlackCard aceOfSwords |> should be False

    [<Fact>]
    let ``Suffle a deck``() =
        let shuffledDeck = shuffleDeck

        shuffledDeck |> should haveLength 40
        shuffledDeck |> should not' (equal cardValues.Keys)

    [<Fact>]
    let ``Check turn winner should return first player when he win``() =
        let playerOne =
            createPlayer "John"
                [ (Ace, Sword)
                  (Ace, Cup)
                  (Seven, Gold) ]
        let playerTwo =
            createPlayer "Mary"
                [ (Eleven, Gold)
                  (Ace, Gold)
                  (Seven, Sword) ]

        let winner = checkTurnWinner (playerOne, (Ace, Sword)) (playerTwo, (Ace, Gold))

        winner |> should not' (equal None)
        winner |> should equal (Some playerOne)

    [<Fact>]
    let ``Check turn winner should return second player when he win``() =
        let playerOne =
            createPlayer "John"
                [ (Ace, Sword)
                  (Ace, Cup)
                  (Seven, Gold) ]
        let playerTwo =
            createPlayer "Mary"
                [ (Eleven, Gold)
                  (Ace, Gold)
                  (Seven, Sword) ]

        let winner = checkTurnWinner (playerOne, (Eleven, Gold)) (playerTwo, (Seven, Gold))

        winner |> should not' (equal None)
        winner |> should equal (Some playerTwo)

    [<Fact>]
    let ``Check turn winner should return none when its draw``() =
        let playerOne =
            createPlayer "John"
                [ (Ace, Sword)
                  (Ace, Cup)
                  (Seven, Gold) ]
        let playerTwo =
            createPlayer "Mary"
                [ (Eleven, Gold)
                  (Ace, Gold)
                  (Seven, Gold) ]

        let winner = checkTurnWinner (playerOne, (Seven, Gold)) (playerTwo, (Seven, Gold))

        winner |> should equal None
