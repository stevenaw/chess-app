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

            var newState = new GameState(newBoard, state.AttackState, move, history);
            var opponentPieces = (endSquare & newBoard.WhitePieces) != 0
                ? newBoard.BlackPieces : newBoard.WhitePieces;

            return AnalyzeAndApplyState(newState, opponentPieces);
        }

        private static GameState AnalyzeAndApplyState(GameState newState, ulong opponentPieces)
        {
            var newBoard = newState.Board;
            var ownPieces = newBoard.AllPieces & ~opponentPieces;
            var ownMovements = MoveGenerator.GenerateStandardMoves(newState, ownPieces, 0);
            var opponentMovements = MoveGenerator.GenerateMoves(newState, opponentPieces, ownMovements);

            var opponentKingUnderAttack = (opponentPieces & newBoard.Kings & ownMovements) != 0;
            var opponentCanMove = opponentMovements != 0;

            var attackState = AttackState.None;
            if (opponentKingUnderAttack)
                attackState = opponentCanMove ? AttackState.Check : AttackState.Checkmate;
            else if (!opponentCanMove)
                attackState = AttackState.Stalemate;
            else
            {
                var count = 0;
                var duplicateCount = 0;

                foreach (var state in newState.PossibleRepeatedHistory)
                {
                    count++;
                    if (BoardState.Equals(state, newBoard))
                        duplicateCount++;
                }

                if (count == Constants.MoveLimits.InactivityLimit)
                    attackState = AttackState.DrawByInactivity;
                else if (duplicateCount >= Constants.MoveLimits.RepetitionLimit)
                    attackState = AttackState.DrawByRepetition;
            }

            return newState.SetAttackState(attackState);
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
