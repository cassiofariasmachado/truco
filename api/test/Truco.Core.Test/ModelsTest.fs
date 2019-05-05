namespace Truco.Core

open Xunit
open FsUnit
open Truco.Core.Models

module ModelsTest =
    [<Fact>]
    let ``Should create a player correctly`` () =
        let name = "John"
        let hand = [(Ace, Sword)]
        let player =  createPlayer name hand
        player.Name |> should equal name
        player.Hand |> should equal hand

    [<Fact>]
    let ``Should create a hand with only three cards`` () =
        let hand = createHand []
        hand |> should equal None

    [<Fact>]
    let ``Create a hand with with an empty cards should return None`` () =
        let cards = [(Ace, Sword); (Ace, Club); (Two, Cup)]
        let hand = createHand cards
        hand |> should equal (Some cards)

    [<Fact>]
    let ``Create a hand with with more than three cards should return None`` () =
        let hand = createHand [(Ace, Sword), (Ace, Club), (Two, Cup), (Ace, Gold)]
        hand |> should equal None

    