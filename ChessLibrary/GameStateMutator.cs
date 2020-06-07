using ChessLibrary.Models;

namespace ChessLibrary
{
    internal static class GameStateMutator
    {
        public static GameState ApplyMove(GameState state, Move move)
        {
            // TODO: Castling stuff. Simplify this

            var startSq = BitTranslator.TranslateToBit(move.StartFile, move.StartRank);
            var endSq = BitTranslator.TranslateToBit(move.EndFile, move.EndRank);

            // Pawn moved in attacking formation, but destination square is empty
            var isEnPassant = ((startSq & state.BoardState.Pawns) != 0
                                && move.StartFile != move.EndFile
                                && (endSq & state.BoardState.AllPieces) == 0);

            var newBoard = state.BoardState.MovePiece(startSq, endSq);
            if (isEnPassant)
            {
                var opponentPawn = BitTranslator.TranslateToBit(move.EndFile, move.StartRank);
                newBoard = BoardStateMutator.ClearPiece(newBoard, opponentPawn);
            }
            else if (move.PromotedPiece != SquareContents.Empty)
            {
                newBoard = newBoard.SetPiece(endSq, move.PromotedPiece);
            }

            return GameState.FromState(newBoard, state.AttackState, move);
        }

        public static GameState ApplyMove(GameState state, ulong startSq, ulong endSq)
        {
            var start = BitTranslator.TranslateToSquare(startSq);
            var end = BitTranslator.TranslateToSquare(endSq);
            var move = new Move(start.File, start.Rank, end.File, end.Rank);

            return ApplyMove(state, move);
        }
    }
}
