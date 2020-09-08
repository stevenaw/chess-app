using System.Collections.Immutable;

namespace ChessLibrary.Models
{
    internal readonly struct GameState
    {
        public readonly BoardState Board;
        public readonly Move PrecedingMove;

        public readonly ImmutableStack<(BoardState, ulong)> PossibleRepeatedHistory;

        public readonly AttackState AttackState;
        public readonly ulong PiecesOnStartSquares;
        public readonly IndexedTuple<ulong> SquaresAttackedBy;

        public GameState(
            BoardState board,
            Move previousMove,
            ImmutableStack<(BoardState, ulong)> history,
            AttackState attackState,
            ulong piecesOnStartSquares,
            IndexedTuple<ulong> squaresAttackedBy
        )
        {
            Board = board;
            AttackState = attackState;
            PrecedingMove = previousMove;
            PossibleRepeatedHistory = history;
            PiecesOnStartSquares = piecesOnStartSquares;
            SquaresAttackedBy = squaresAttackedBy;
        }

        public GameState SetAttackState(AttackState state, IndexedTuple<ulong> squaresAttackedBy)
        {
            return new GameState(Board, PrecedingMove, PossibleRepeatedHistory, state, PiecesOnStartSquares, squaresAttackedBy);
        }

        public static GameState Initialize(BoardState boardState)
        {
            return new GameState(
                boardState,
                Move.Empty,
                ImmutableStack<(BoardState, ulong)>.Empty,
                AttackState.None,
                boardState.AllPieces,
                IndexedTuple<ulong>.Empty
            );
        }
    }
}
