namespace Truco.Core

open Xunit
open FsUnit
open Truco.Core.Models
open Truco.Core.Actions
open Truco.Core.GameLoop

module GameLoopTest =

    // Helper function to create a test match with players
    let createTestMatch() =
        let playerOne = createPlayer "Alice" [(Ace, Sword); (Seven, Gold); (Three, Club)]
        let playerTwo = createPlayer "Bob" [(Ace, Club); (Seven, Sword); (Two, Gold)]
        createMatch playerOne playerTwo

    // Helper function to create a turn
    let createTestTurn playerOneName playerTwoName winnerName =
        let playerOne = createPlayer playerOneName []
        let playerTwo = createPlayer playerTwoName []
        let winner = if winnerName = playerOneName then Some playerOne elif winnerName = playerTwoName then Some playerTwo else None
        createTurn 
            (createPlayerMove playerOne (Ace, Sword))
            (createPlayerMove playerTwo (Ace, Club))
            winner

    [<Fact>]
    let ``determineRoundWinner should return None when no turns played``() =
        let round = createRound
        let result = determineRoundWinner round
        result |> should equal None

    [<Fact>]
    let ``determineRoundWinner should return None after one turn win``() =
        let turn1 = createTestTurn "Alice" "Bob" "Alice"
        let round = { createRound with TurnHistory = [turn1] }
        let result = determineRoundWinner round
        result |> should equal None

    [<Fact>]
    let ``determineRoundWinner should return player one after winning 2 turns``() =
        let turn1 = createTestTurn "Alice" "Bob" "Alice"
        let turn2 = createTestTurn "Alice" "Bob" "Alice"
        let round = { createRound with TurnHistory = [turn2; turn1] }
        let result = determineRoundWinner round
        
        result |> should not' (equal None)
        match result with
        | Some (RoundWinner winner) -> winner.Name |> should equal "Alice"
        | None -> failwith "Expected a winner"

    [<Fact>]
    let ``determineRoundWinner should return player two after winning 2 turns``() =
        let turn1 = createTestTurn "Alice" "Bob" "Bob"
        let turn2 = createTestTurn "Alice" "Bob" "Bob"
        let round = { createRound with TurnHistory = [turn2; turn1] }
        let result = determineRoundWinner round
        
        result |> should not' (equal None)
        match result with
        | Some (RoundWinner winner) -> winner.Name |> should equal "Bob"
        | None -> failwith "Expected a winner"

    [<Fact>]
    let ``determineRoundWinner should return winner after 2-1 split``() =
        let turn1 = createTestTurn "Alice" "Bob" "Alice"
        let turn2 = createTestTurn "Alice" "Bob" "Bob"
        let turn3 = createTestTurn "Alice" "Bob" "Alice"
        let round = { createRound with TurnHistory = [turn3; turn2; turn1] }
        let result = determineRoundWinner round
        
        result |> should not' (equal None)
        match result with
        | Some (RoundWinner winner) -> winner.Name |> should equal "Alice"
        | None -> failwith "Expected a winner"

    [<Fact>]
    let ``determineRoundWinner should return None when all turns are draws``() =
        let turn1 = createTestTurn "Alice" "Bob" ""
        let turn2 = createTestTurn "Alice" "Bob" ""
        let turn3 = createTestTurn "Alice" "Bob" ""
        let round = { createRound with TurnHistory = [turn3; turn2; turn1] }
        let result = determineRoundWinner round
        result |> should equal None

    [<Fact>]
    let ``awardRoundPoints should add point to player one when they win``() =
        let _match = createTestMatch()
        let winner = RoundWinner _match.PlayerOne.Player
        let updatedMatch = awardRoundPoints _match winner
        
        updatedMatch.PlayerOne.Points |> should equal 1
        updatedMatch.PlayerTwo.Points |> should equal 0

    [<Fact>]
    let ``awardRoundPoints should add point to player two when they win``() =
        let _match = createTestMatch()
        let winner = RoundWinner _match.PlayerTwo.Player
        let updatedMatch = awardRoundPoints _match winner
        
        updatedMatch.PlayerOne.Points |> should equal 0
        updatedMatch.PlayerTwo.Points |> should equal 1

    [<Fact>]
    let ``awardRoundPoints should preserve existing points``() =
        let _match = createTestMatch()
        let matchWithPoints = 
            { _match with 
                PlayerOne = { _match.PlayerOne with Points = 5 }
                PlayerTwo = { _match.PlayerTwo with Points = 3 } }
        
        let winner = RoundWinner matchWithPoints.PlayerOne.Player
        let updatedMatch = awardRoundPoints matchWithPoints winner
        
        updatedMatch.PlayerOne.Points |> should equal 6
        updatedMatch.PlayerTwo.Points |> should equal 3

    [<Fact>]
    let ``dealNewHands should give each player 3 cards``() =
        let _match = createTestMatch()
        let updatedMatch = dealNewHands _match
        
        updatedMatch.PlayerOne.Player.Hand |> should haveLength 3
        updatedMatch.PlayerTwo.Player.Hand |> should haveLength 3

    [<Fact>]
    let ``dealNewHands should give different cards to each player``() =
        let _match = createTestMatch()
        let updatedMatch = dealNewHands _match
        
        let playerOneCards = updatedMatch.PlayerOne.Player.Hand
        let playerTwoCards = updatedMatch.PlayerTwo.Player.Hand
        
        // Check that players don't have the exact same hand
        playerOneCards |> should not' (equal playerTwoCards)

    [<Fact>]
    let ``dealNewHands should replace existing hands``() =
        let _match = createTestMatch()
        let originalPlayerOneHand = _match.PlayerOne.Player.Hand
        let updatedMatch = dealNewHands _match
        
        updatedMatch.PlayerOne.Player.Hand |> should not' (equal originalPlayerOneHand)

    [<Fact>]
    let ``checkMatchWinner should return None when no one reached target``() =
        let _match = createTestMatch()
        let result = checkMatchWinner _match 12
        result |> should equal None

    [<Fact>]
    let ``checkMatchWinner should return player one when they reach target score``() =
        let _match = createTestMatch()
        let matchWithPoints = 
            { _match with PlayerOne = { _match.PlayerOne with Points = 12 } }
        
        let result = checkMatchWinner matchWithPoints 12
        
        result |> should not' (equal None)
        match result with
        | Some (MatchWinner winner) -> winner.Name |> should equal "Alice"
        | None -> failwith "Expected a winner"

    [<Fact>]
    let ``checkMatchWinner should return player two when they reach target score``() =
        let _match = createTestMatch()
        let matchWithPoints = 
            { _match with PlayerTwo = { _match.PlayerTwo with Points = 12 } }
        
        let result = checkMatchWinner matchWithPoints 12
        
        result |> should not' (equal None)
        match result with
        | Some (MatchWinner winner) -> winner.Name |> should equal "Bob"
        | None -> failwith "Expected a winner"

    [<Fact>]
    let ``checkMatchWinner should return player one when both reach target but player one is higher``() =
        let _match = createTestMatch()
        let matchWithPoints = 
            { _match with 
                PlayerOne = { _match.PlayerOne with Points = 13 }
                PlayerTwo = { _match.PlayerTwo with Points = 12 } }
        
        let result = checkMatchWinner matchWithPoints 12
        
        result |> should not' (equal None)
        match result with
        | Some (MatchWinner winner) -> winner.Name |> should equal "Alice"
        | None -> failwith "Expected a winner"

    [<Fact>]
    let ``checkMatchWinner should return None when scores are below target``() =
        let _match = createTestMatch()
        let matchWithPoints = 
            { _match with 
                PlayerOne = { _match.PlayerOne with Points = 10 }
                PlayerTwo = { _match.PlayerTwo with Points = 11 } }
        
        let result = checkMatchWinner matchWithPoints 12
        result |> should equal None

    [<Fact>]
    let ``startNewRound should add new round to history``() =
        let _match = createTestMatch()
        let updatedMatch = startNewRound _match
        
        updatedMatch.RoundHistory |> should haveLength 1
        updatedMatch.RoundHistory.Head.TurnHistory |> should haveLength 0

    [<Fact>]
    let ``startNewRound should deal new hands to both players``() =
        let _match = createTestMatch()
        let originalPlayerOneHand = _match.PlayerOne.Player.Hand
        let updatedMatch = startNewRound _match
        
        updatedMatch.PlayerOne.Player.Hand |> should not' (equal originalPlayerOneHand)
        updatedMatch.PlayerOne.Player.Hand |> should haveLength 3
        updatedMatch.PlayerTwo.Player.Hand |> should haveLength 3

    [<Fact>]
    let ``startNewRound should preserve existing round history``() =
        let _match = createTestMatch()
        let matchWithRounds = 
            { _match with RoundHistory = [createRound] }
        
        let updatedMatch = startNewRound matchWithRounds
        
        updatedMatch.RoundHistory |> should haveLength 2

    [<Fact>]
    let ``startNewRound should preserve player points``() =
        let _match = createTestMatch()
        let matchWithPoints = 
            { _match with 
                PlayerOne = { _match.PlayerOne with Points = 5 }
                PlayerTwo = { _match.PlayerTwo with Points = 3 } }
        
        let updatedMatch = startNewRound matchWithPoints
        
        updatedMatch.PlayerOne.Points |> should equal 5
        updatedMatch.PlayerTwo.Points |> should equal 3

    [<Fact>]
    let ``playTurn should delegate to turn function from Actions``() =
        let playerOne = createPlayer "Alice" [(Ace, Sword); (Seven, Gold); (Three, Club)]
        let playerTwo = createPlayer "Bob" [(Ace, Club); (Seven, Sword); (Two, Gold)]
        let _match = 
            { createMatch playerOne playerTwo with 
                RoundHistory = [createRound] }
        
        let cardOne = (Ace, Sword)
        let cardTwo = (Ace, Club)
        
        let updatedMatch = playTurn _match cardOne cardTwo
        
        // Verify turn was added to round history
        updatedMatch.RoundHistory.Head.TurnHistory |> should haveLength 1
        
        // Verify cards were removed from hands
        updatedMatch.PlayerOne.Player.Hand |> should not' (contain cardOne)
        updatedMatch.PlayerTwo.Player.Hand |> should not' (contain cardTwo)

    [<Fact>]
    let ``getFirstPlayer should return player one when no rounds played``() =
        let _match = createTestMatch()
        let firstPlayer = getFirstPlayer _match
        firstPlayer.Name |> should equal "Alice"

    [<Fact>]
    let ``getFirstPlayer should return player one when current round has no turns``() =
        let _match = 
            { createTestMatch() with RoundHistory = [createRound] }
        let firstPlayer = getFirstPlayer _match
        firstPlayer.Name |> should equal "Alice"

    [<Fact>]
    let ``getFirstPlayer should return winner of last turn``() =
        let playerOne = createPlayer "Alice" []
        let playerTwo = createPlayer "Bob" []
        let turn = 
            createTurn 
                (createPlayerMove playerTwo (Ace, Sword))
                (createPlayerMove playerOne (Two, Gold))
                (Some playerTwo)
        let round = { createRound with TurnHistory = [turn] }
        let _match = 
            { createTestMatch() with RoundHistory = [round] }
        
        let firstPlayer = getFirstPlayer _match
        firstPlayer.Name |> should equal "Bob"

    [<Fact>]
    let ``getFirstPlayer should return player one when last turn was a draw``() =
        let turn = createTestTurn "Alice" "Bob" ""
        let round = { createRound with TurnHistory = [turn] }
        let _match = 
            { createTestMatch() with RoundHistory = [round] }
        
        let firstPlayer = getFirstPlayer _match
        firstPlayer.Name |> should equal "Alice"
