using System.Collections.Immutable;

namespace ChessLibrary.Models
{
    internal readonly struct GameState
    {
        public readonly ImmutableStack<BoardState> PossibleRepeatedHistory { get; }
        public readonly BoardState BoardState { get; }
        public readonly AttackState AttackState { get; }
        public readonly Move PrecedingMove { get; }

        public GameState(BoardState board, AttackState attackState, Move previousMove, ImmutableStack<BoardState> history)
        {
            BoardState = board;
            AttackState = attackState;
            PrecedingMove = previousMove;
            PossibleRepeatedHistory = history;
        }

        public GameState SetAttackState(AttackState state)
        {
            return new GameState(BoardState, state, PrecedingMove, PossibleRepeatedHistory);
        }

        public static GameState Initialize(BoardState boardState)
        {
            return new GameState(boardState, AttackState.None, Move.Empty, ImmutableStack<BoardState>.Empty);
        }
    }
}
