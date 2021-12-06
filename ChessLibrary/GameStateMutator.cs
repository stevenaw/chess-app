using ChessLibrary.Models;

namespace ChessLibrary
{
    internal static class GameStateMutator
    {
        public static GameState ApplyMove(GameState state, Move move, ulong startSquare, ulong endSquare)
        {
            var history = state.PossibleRepeatedHistory;
            var resetHistory = (
                    ((state.Board.AllPieces & endSquare) != 0)     // regular capture 
                    || ((state.Board.Pawns & startSquare) != 0)    // pawn movement
                );

            if (resetHistory)
                history = history.Clear();


            var newBoard = state.Board.MovePiece(startSquare, endSquare);

            if ((startSquare & state.Board.Pawns) != 0)
            {
                if (endSquare == state.EnPassantSquare)
                {
                    var opponentPawn = BitTranslator.TranslateToBit(move.EndFile, move.StartRank);
                    newBoard = BoardStateMutator.ClearPiece(newBoard, opponentPawn);
                }
                else if (move.PromotedPiece != SquareContents.Empty)
                {
                    newBoard = newBoard.ClearPiece(endSquare).SetPiece(endSquare, move.PromotedPiece);
                }
            }
            else if ((startSquare & state.Board.Kings) != 0)
            {
                // Castling
                if (Math.Abs(move.EndFile - move.StartFile) == 2)
                {
                    var rookDestinationRelativeKing = Math.Sign(move.StartFile - move.EndFile);
                    var rookDestinationFile = (char)(move.EndFile + rookDestinationRelativeKing);
                    var rookDestinationBit = BitTranslator.TranslateToBit(rookDestinationFile, move.EndRank);

                    var rookOriginFile = rookDestinationRelativeKing > 0 ? 'a' : 'h';
                    var rookOriginBit = BitTranslator.TranslateToBit(rookOriginFile, move.EndRank);

                    newBoard = BoardStateMutator.MovePiece(newBoard, rookOriginBit, rookDestinationBit);
                }
            }


            var newPiecesStillOnStartSquare = state.PiecesOnStartSquares & ~(startSquare | endSquare);
            var castlingPiecesUnmoved = newPiecesStillOnStartSquare & MoveGenerator.StartingKingsAndRooks;

            var enPassantSquare = 0UL;
            if ((startSquare & state.Board.Pawns) != 0)
            {
                if (startSquare >> 16 == endSquare)
                    enPassantSquare = startSquare >> 8;
                else if (startSquare << 16 == endSquare)
                    enPassantSquare = startSquare << 8;
            }

            history = history.Push((newBoard, castlingPiecesUnmoved));

            return new GameState(newBoard, move, history, state.AttackState, newPiecesStillOnStartSquare, enPassantSquare, state.SquaresAttackedBy);
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
