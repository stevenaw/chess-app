using ChessLibrary.Models;
using System;

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
            
            // Pawn moved in attacking formation, but destination square is empty
            var isEnPassant = ((startSquare & state.Board.Pawns) != 0
                                && move.StartFile != move.EndFile
                                && (endSquare & state.Board.AllPieces) == 0);
            var isCastling = (startSquare & state.Board.Kings) != 0
                                && Math.Abs(move.EndFile - move.StartFile) == 2;

            if (isEnPassant)
            {
                var opponentPawn = BitTranslator.TranslateToBit(move.EndFile, move.StartRank);
                newBoard = BoardStateMutator.ClearPiece(newBoard, opponentPawn);
            }
            else if (move.PromotedPiece != SquareContents.Empty)
            {
                newBoard = newBoard.ClearPiece(endSquare).SetPiece(endSquare, move.PromotedPiece);
            }
            else if (isCastling)
            {
                var rookDestinationRelativeKing = Math.Sign(move.StartFile - move.EndFile);
                var rookDestinationFile = (char)(move.EndFile + rookDestinationRelativeKing);
                var rookDestinationBit = BitTranslator.TranslateToBit(rookDestinationFile, move.EndRank);

                var rookOriginFile = rookDestinationRelativeKing > 0 ? 'a' : 'h';
                var rookOriginBit = BitTranslator.TranslateToBit(rookOriginFile, move.EndRank);

                newBoard = BoardStateMutator.MovePiece(newBoard, rookOriginBit, rookDestinationBit);
            }


            var newPiecesStillOnStartSquare = state.PiecesOnStartSquares & ~(startSquare | endSquare);
            var castlingPiecesUnmoved = newPiecesStillOnStartSquare & MoveGenerator.StartingKingsAndRooks;

            history = history.Push((newBoard, castlingPiecesUnmoved));

            return new GameState(newBoard, move, history, state.AttackState, newPiecesStillOnStartSquare, state.SquaresAttackedBy);
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
