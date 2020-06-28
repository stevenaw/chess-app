using System.Collections.Immutable;

namespace ChessLibrary.Models
{
    internal readonly struct GameState
    {
        public readonly BoardState Board { get; }
        public readonly Move PrecedingMove { get; }
        public readonly ImmutableStack<BoardState> PossibleRepeatedHistory { get; }

        public readonly AttackState AttackState { get; }
        public readonly ulong PiecesOnStartSquares { get; }
        public readonly IndexedTuple<ulong> SquaresAttackedBy { get; }

        public GameState(
            BoardState board,
            Move previousMove,
            ImmutableStack<BoardState> history,
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

        public GameState SetAttackState(AttackState state, ulong piecesOnStartSquares, IndexedTuple<ulong> squaresAttackedBy)
        {
            return new GameState(Board, PrecedingMove, PossibleRepeatedHistory, state, piecesOnStartSquares, squaresAttackedBy);
        }

        public static GameState Initialize(BoardState boardState)
        {
            return new GameState(
                boardState,
                Move.Empty,
                ImmutableStack<BoardState>.Empty,
                AttackState.None,
                boardState.AllPieces,
                IndexedTuple<ulong>.Empty
            );
        }
    }
}
