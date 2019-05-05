namespace Truco.Core

open Xunit
open FsUnit
open Truco.Core.Actions
open Truco.Core.Models

module ActionsTest =
    [<Fact>]
    let ``Card value of Ace of Swords should be 14`` () =
        let aceOfSwords =  (Ace, Sword)
        cardValue aceOfSwords |> should equal 14

    [<Fact>]
    let ``Card value of Four Cup should be 1`` () =
        let fourCup = (Four, Cup)
        cardValue fourCup |> should equal 1

    [<Fact>]
    let ``Four Cup is a black card`` () =
        let fourCup = (Four, Cup)
        isBlackCard fourCup |> should be True

    [<Fact>]
    let ``Ace of Swords isn't a black card`` () =
        let aceOfSwords =  (Ace, Sword)
        isBlackCard aceOfSwords |> should be False

    [<Fact>]
    let ``Suffle a deck`` () =
        let shuffledDeck = shuffleDeck

        shuffledDeck |> should haveLength 40
        shuffledDeck |> should not' (equal cardValues.Keys)
