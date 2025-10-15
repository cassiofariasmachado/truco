namespace Truco.Core

open Xunit
open FsUnit
open Truco.Core.Models
open Truco.Core.GameIO

module GameIOTest =

    [<Fact>]
    let ``GameIO record should have all required functions`` () =
        let mutable messageShown = ""
        let mutable handShown: Player option = None
        let mutable cardChoice = 0
        let mutable turnShown: Turn option = None
        let mutable roundShown: Round option = None
        let mutable matchShown: Match option = None
        let mutable scoreShown: (MatchPlayer * MatchPlayer) option = None

        let testIO: GameIO = {
            ShowMessage = fun msg -> messageShown <- msg
            ShowHand = fun player -> handShown <- Some player
            GetCardChoice = fun _ -> cardChoice
            ShowTurnResult = fun turn -> turnShown <- Some turn
            ShowRoundResult = fun round p1 p2 -> roundShown <- Some round
            ShowMatchResult = fun _match -> matchShown <- Some _match
            ShowScore = fun p1 p2 -> scoreShown <- Some (p1, p2)
        }

        // Test ShowMessage
        testIO.ShowMessage "test"
        messageShown |> should equal "test"

        // Test ShowHand
        let player = createPlayer "Test" []
        testIO.ShowHand player
        handShown |> should equal (Some player)

        // Test GetCardChoice
        let choice = testIO.GetCardChoice player
        choice |> should equal 0

    [<Fact>]
    let ``GameIO record functions should be composable`` () =
        let messages = ResizeArray<string>()

        let testIO: GameIO = {
            ShowMessage = fun msg -> messages.Add(msg)
            ShowHand = fun _ -> ()
            GetCardChoice = fun _ -> 0
            ShowTurnResult = fun _ -> ()
            ShowRoundResult = fun _ _ _ -> ()
            ShowMatchResult = fun _ -> ()
            ShowScore = fun _ _ -> ()
        }

        testIO.ShowMessage "First"
        testIO.ShowMessage "Second"
        testIO.ShowMessage "Third"

        messages.Count |> should equal 3
        messages.[0] |> should equal "First"
        messages.[1] |> should equal "Second"
        messages.[2] |> should equal "Third"

    [<Fact>]
    let ``GameIO record can be partially applied`` () =
        let showMessageWithPrefix prefix msg =
            sprintf "%s: %s" prefix msg

        let mutable output = ""

        let testIO: GameIO = {
            ShowMessage = fun msg -> output <- msg
            ShowHand = fun _ -> ()
            GetCardChoice = fun _ -> 0
            ShowTurnResult = fun _ -> ()
            ShowRoundResult = fun _ _ _ -> ()
            ShowMatchResult = fun _ -> ()
            ShowScore = fun _ _ -> ()
        }

        let prefixedShow = showMessageWithPrefix "INFO"
        testIO.ShowMessage (prefixedShow "Test message")

        output |> should equal "INFO: Test message"

