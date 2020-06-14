using ChessLibrary.Models;

namespace ChessLibrary
{
    internal static class GameStateMutator
    {
        public static GameState ApplyMove(GameState state, Move move, ulong startSquare, ulong endSquare)
        {
            // TODO: Castling
            var history = state.PossibleRepeatedHistory;
            var resetHistory = (
                    ((state.Board.AllPieces & endSquare) != 0)     // regular capture 
                    || ((state.Board.Pawns & startSquare) != 0)    // pawn movement
                );

            if (resetHistory)
                history = history.Clear();


            var newBoard = state.Board.MovePiece(startSquare, endSquare);
            
            // Pawn moved in attacking formation, but destination square is empty
            var isEnPassant = ((startSquare & state.Board.Pawns) != 0
                                && move.StartFile != move.EndFile
                                && (endSquare & state.Board.AllPieces) == 0);
            if (isEnPassant)
            {
                var opponentPawn = BitTranslator.TranslateToBit(move.EndFile, move.StartRank);
                newBoard = BoardStateMutator.ClearPiece(newBoard, opponentPawn);
            }
            else if (move.PromotedPiece != SquareContents.Empty)
            {
                newBoard = newBoard.SetPiece(endSquare, move.PromotedPiece);
            }

            history = history.Push(newBoard);

            return new GameState(newBoard, state.AttackState, move, history);
        }

        public static GameState ApplyMove(GameState state, ulong startSq, ulong endSq)
        {
            var start = BitTranslator.TranslateToSquare(startSq);
            var end = BitTranslator.TranslateToSquare(endSq);
            var move = new Move(start.File, start.Rank, end.File, end.Rank);

            return ApplyMove(state, move, startSq, endSq);
        }
    }
}
