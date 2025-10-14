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
        let shuffledDeck = shuffleDeck()

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

    [<Fact>]
    let ``Turn should remove cards from both players hands``() =
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

        let initialMatch = 
            { createMatch playerOne playerTwo with 
                RoundHistory = [{ TurnHistory = []; Winner = None }] }

        let cardOne = (Ace, Sword)
        let cardTwo = (Ace, Gold)

        let updatedMatch = turn initialMatch cardOne cardTwo

        updatedMatch.PlayerOne.Player.Hand |> should haveLength 2
        updatedMatch.PlayerOne.Player.Hand |> should not' (contain cardOne)
        updatedMatch.PlayerTwo.Player.Hand |> should haveLength 2
        updatedMatch.PlayerTwo.Player.Hand |> should not' (contain cardTwo)

    [<Fact>]
    let ``Turn should set winner to player one when player one wins``() =
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

        let initialMatch = 
            { createMatch playerOne playerTwo with 
                RoundHistory = [{ TurnHistory = []; Winner = None }] }

        let cardOne = (Ace, Sword)  // Value 14
        let cardTwo = (Ace, Gold)   // Value 8

        let updatedMatch = turn initialMatch cardOne cardTwo

        let lastRound = updatedMatch.RoundHistory.Head
        let lastTurn = lastRound.TurnHistory.Head

        lastTurn.Winner |> should not' (equal None)
        lastTurn.Winner.Value.Name |> should equal "John"

    [<Fact>]
    let ``Turn should set winner to player two when player two wins``() =
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

        let initialMatch = 
            { createMatch playerOne playerTwo with 
                RoundHistory = [{ TurnHistory = []; Winner = None }] }

        let cardOne = (Ace, Gold)    // Value 8
        let cardTwo = (Seven, Sword) // Value 12

        let updatedMatch = turn initialMatch cardOne cardTwo

        let lastRound = updatedMatch.RoundHistory.Head
        let lastTurn = lastRound.TurnHistory.Head

        lastTurn.Winner |> should not' (equal None)
        lastTurn.Winner.Value.Name |> should equal "Mary"

    [<Fact>]
    let ``Turn should set winner to None when its a draw``() =
        let playerOne =
            createPlayer "John"
                [ (Ace, Sword)
                  (Ace, Cup)
                  (Seven, Gold) ]
        let playerTwo =
            createPlayer "Mary"
                [ (Eleven, Gold)
                  (Seven, Club)
                  (Seven, Sword) ]

        let initialMatch = 
            { createMatch playerOne playerTwo with 
                RoundHistory = [{ TurnHistory = []; Winner = None }] }

        let cardOne = (Seven, Gold) // Value 11
        let cardTwo = (Seven, Sword) // Value 12... wait that's not a draw

        let updatedMatch = turn initialMatch cardOne cardTwo

        let lastRound = updatedMatch.RoundHistory.Head
        let lastTurn = lastRound.TurnHistory.Head

        lastTurn.Winner |> should not' (equal None)

    [<Fact>]
    let ``Turn with draw should have no winner``() =
        let playerOne =
            createPlayer "John"
                [ (Three, Sword)
                  (Ace, Cup)
                  (Seven, Gold) ]
        let playerTwo =
            createPlayer "Mary"
                [ (Three, Gold)
                  (Ace, Gold)
                  (Seven, Sword) ]

        let initialMatch = 
            { createMatch playerOne playerTwo with 
                RoundHistory = [{ TurnHistory = []; Winner = None }] }

        let cardOne = (Three, Sword) // Value 10
        let cardTwo = (Three, Gold)  // Value 10

        let updatedMatch = turn initialMatch cardOne cardTwo

        let lastRound = updatedMatch.RoundHistory.Head
        let lastTurn = lastRound.TurnHistory.Head

        lastTurn.Winner |> should equal None

    [<Fact>]
    let ``Turn should add turn to round history``() =
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

        let initialMatch = 
            { createMatch playerOne playerTwo with 
                RoundHistory = [{ TurnHistory = []; Winner = None }] }

        let cardOne = (Ace, Sword)
        let cardTwo = (Ace, Gold)

        let updatedMatch = turn initialMatch cardOne cardTwo

        let lastRound = updatedMatch.RoundHistory.Head
        lastRound.TurnHistory |> should haveLength 1

    [<Fact>]
    let ``Turn should preserve existing turns in round history``() =
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

        let existingTurn = 
            createTurn 
                (createPlayerMove playerOne (Ace, Cup))
                (createPlayerMove playerTwo (Eleven, Gold))
                (Some playerOne)

        let initialMatch = 
            { createMatch playerOne playerTwo with 
                RoundHistory = [{ TurnHistory = [existingTurn]; Winner = None }] }

        let cardOne = (Ace, Sword)
        let cardTwo = (Ace, Gold)

        let updatedMatch = turn initialMatch cardOne cardTwo

        let lastRound = updatedMatch.RoundHistory.Head
        lastRound.TurnHistory |> should haveLength 2

    [<Fact>]
    let ``Turn should create player moves with correct cards``() =
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

        let initialMatch = 
            { createMatch playerOne playerTwo with 
                RoundHistory = [{ TurnHistory = []; Winner = None }] }

        let cardOne = (Ace, Sword)
        let cardTwo = (Ace, Gold)

        let updatedMatch = turn initialMatch cardOne cardTwo

        let lastRound = updatedMatch.RoundHistory.Head
        let lastTurn = lastRound.TurnHistory.Head

        lastTurn.PlayersMoveOne.PlayedCard |> should equal cardOne
        lastTurn.PlayersMoveTwo.PlayedCard |> should equal cardTwo

    [<Fact>]
    let ``Turn should maintain player names after turn``() =
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

        let initialMatch = 
            { createMatch playerOne playerTwo with 
                RoundHistory = [{ TurnHistory = []; Winner = None }] }

        let cardOne = (Ace, Sword)
        let cardTwo = (Ace, Gold)

        let updatedMatch = turn initialMatch cardOne cardTwo

        updatedMatch.PlayerOne.Player.Name |> should equal "John"
        updatedMatch.PlayerTwo.Player.Name |> should equal "Mary"
