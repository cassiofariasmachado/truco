namespace Truco.Core

open Truco.Core.Models

module GameIO =

    /// <summary>
    ///   Record of functions for game I/O operations (more idiomatic F#)
    /// </summary>
    type GameIO = {
        ShowMessage: string -> unit
        ShowHand: Player -> unit
        GetCardChoice: Player -> int
        ShowTurnResult: Turn -> unit
        ShowRoundResult: Round -> MatchPlayer -> MatchPlayer -> unit
        ShowMatchResult: Match -> unit
        ShowScore: MatchPlayer -> MatchPlayer -> unit
    }


