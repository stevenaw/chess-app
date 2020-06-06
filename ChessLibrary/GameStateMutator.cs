using ChessLibrary.Models;

namespace ChessLibrary
{
    internal static class GameStateMutator
    {
        public static GameState ApplyMove(GameState state, Move move)
        {
            // TODO: Castling stuff. Simplify this
            // TODO: Ensure we clear old state on en passant

            var startSq = BitTranslator.TranslateToBit(move.StartFile, move.StartRank);
            var endSq = BitTranslator.TranslateToBit(move.EndFile, move.EndRank);

            var newBoard = state.BoardState.MovePiece(startSq, endSq);
            if (move.PromotedPiece != SquareContents.Empty)
                newBoard = newBoard.SetPiece(endSq, move.PromotedPiece);

            var newState = GameState.FromState(newBoard, state.AttackState, move);

            return newState;
        }

        public static GameState ApplyMove(GameState state, ulong startSq, ulong endSq)
        {
            var start = BitTranslator.TranslateToSquare(startSq);
            var end = BitTranslator.TranslateToSquare(endSq);
            var move = new Move()
            {
                StartFile = start.File,
                StartRank = start.Rank,
                EndFile = end.File,
                EndRank = end.Rank
            };

            return ApplyMove(state, move);
        }
    }
}
