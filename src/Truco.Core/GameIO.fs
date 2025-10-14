namespace Truco.Core

open Truco.Core.Models

module GameIO =

    /// <summary>
    ///   Abstract type for game I/O operations
    /// </summary>
    type IGameIO =
        abstract member ShowMessage: string -> unit
        abstract member ShowHand: Player -> unit
        abstract member GetCardChoice: Player -> int
        abstract member ShowTurnResult: Turn -> unit
        abstract member ShowRoundResult: Round -> MatchPlayer -> MatchPlayer -> unit
        abstract member ShowMatchResult: Match -> unit
        abstract member ShowScore: MatchPlayer -> MatchPlayer -> unit
