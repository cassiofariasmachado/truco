namespace Truco.Core

module Models =

    type Suit =
        | Sword
        | Club
        | Gold
        | Cup

    type Rank =
        | Ace
        | Two
        | Three
        | Four
        | Five
        | Six
        | Seven
        | Eight
        | Nine
        | Ten
        | Eleven
        | Twelve

    type Card = Rank * Suit

    type Hand = Card list

    type Player = { Name: string; Hand: Hand }

    let createPlayer name hand = { Name = name; Hand = hand }

    type Deck = Card list

    type PlayersMove = { Player: Player; PlayedCard: Card }

    let createPlayerMove player card = { Player = player; PlayedCard = card }

    type TurnWinner = Player option

    type Turn =
        { PlayersMoveOne: PlayersMove
          PlayersMoveTwo: PlayersMove
          Winner: TurnWinner }

    let createTurn playerOneMove playerTwoMove winner =
        { PlayersMoveOne = playerOneMove
          PlayersMoveTwo = playerTwoMove
          Winner = winner }

    type RoundWinner = RoundWinner of Player

    type Round =
        { TurnHistory: Turn list
          Winner: RoundWinner option }

    let createRound = { TurnHistory = []; Winner = None }

    type MatchPlayer = { Player: Player; Points: int }

    let createMatchPlayer player = { Player = player; Points = 0 }

    type MatchWinner = MatchWinner of Player

    type Match =
        { PlayerOne: MatchPlayer
          PlayerTwo: MatchPlayer
          RoundHistory: Round list
          Winner: MatchWinner option }

    let createMatch playerOne playerTwo =
        { PlayerOne = createMatchPlayer playerOne
          PlayerTwo = createMatchPlayer playerTwo
          RoundHistory = []
          Winner = None }

    // Semantic constants for special card values
    let highestCardValue = 14
    let secondHighestCardValue = 13
    let thirdHighestCardValue = 12
    let fourthHighestCardValue = 11

    let cardValues =
        [ (Ace, Sword), 14
          (Ace, Club), 13
          (Seven, Sword), 12
          (Seven, Gold), 11

          (Three, Sword), 10
          (Three, Club), 10
          (Three, Gold), 10
          (Three, Cup), 10

          (Two, Sword), 9
          (Two, Club), 9
          (Two, Gold), 9
          (Two, Cup), 9

          (Ace, Gold), 8
          (Ace, Cup), 8

          (Twelve, Sword), 7
          (Twelve, Club), 7
          (Twelve, Gold), 7
          (Twelve, Cup), 7

          (Eleven, Sword), 6
          (Eleven, Club), 6
          (Eleven, Gold), 6
          (Eleven, Cup), 6

          (Ten, Sword), 5
          (Ten, Club), 5
          (Ten, Gold), 5
          (Ten, Cup), 5

          (Seven, Club), 4
          (Seven, Cup), 4

          (Six, Sword), 3
          (Six, Club), 3
          (Six, Gold), 3
          (Six, Cup), 3

          (Five, Sword), 2
          (Five, Club), 2
          (Five, Gold), 2
          (Five, Cup), 2

          (Four, Sword), 1
          (Four, Club), 1
          (Four, Gold), 1
          (Four, Cup), 1 ]
        |> Map.ofList
