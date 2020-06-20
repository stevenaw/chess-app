using System.Collections.Immutable;

namespace ChessLibrary.Models
{
    internal readonly struct GameState
    {
        public readonly BoardState Board { get; }
        public readonly Move PrecedingMove { get; }
        public readonly ImmutableStack<BoardState> PossibleRepeatedHistory { get; }

        public readonly AttackState AttackState { get; }
        public readonly IndexedTuple<bool> HasKingMoved { get; }

        public GameState(
            BoardState board,
            Move previousMove,
            ImmutableStack<BoardState> history,
            AttackState attackState,
            IndexedTuple<bool> hasKingMoved
        )
        {
            Board = board;
            AttackState = attackState;
            PrecedingMove = previousMove;
            PossibleRepeatedHistory = history;
            HasKingMoved = hasKingMoved;
        }

        public GameState SetAttackState(AttackState state, IndexedTuple<bool> hasKingMoved)
        {
            return new GameState(Board, PrecedingMove, PossibleRepeatedHistory, state, hasKingMoved);
        }

        public static GameState Initialize(BoardState boardState)
        {
            return new GameState(
                boardState,
                Move.Empty,
                ImmutableStack<BoardState>.Empty,
                AttackState.None,
                IndexedTuple<bool>.Empty
            );
        }
    }
}
