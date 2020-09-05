using ChessLibrary.Models;
using System;
using System.Collections.Generic;

namespace ChessLibrary
{
    public sealed class Game
    {
        internal GameState CurrentState { get; private set; }
        internal Stack<GameState> History { get; set; } = new Stack<GameState>();

        private ulong CurrentTurn { get; set; }
        public AttackState AttackState { get { return CurrentState.AttackState; } }

        public Game() : this(BoardState.DefaultPositions, PieceColor.White)
        { }

        internal Game(BoardState state) : this(state, PieceColor.White)
        { }

        internal Game(BoardState state, PieceColor turn)
        {
            CurrentState = GameState.Initialize(state);
            CurrentTurn = turn == PieceColor.White ? CurrentState.Board.WhitePieces : CurrentState.Board.BlackPieces;
            History.Push(CurrentState);
        }

        public PieceColor GetTurn()
        {
            return (CurrentTurn & CurrentState.Board.WhitePieces) != 0
                ? PieceColor.White
                : PieceColor.Black;
        }

        public SquareContents GetSquareContents(char file, int rank)
        {
            var result = (SquareContents)0;

            if (BitTranslator.IsValidSquare(file, rank))
            {
                ulong bit = BitTranslator.TranslateToBit(file, rank);
                result = CurrentState.Board.GetSquareContents(bit);
            }

            return result;
        }

        public ErrorCondition Move(string input)
        {
            if (MoveParser.TryParseMove(input, CurrentState.Board, CurrentTurn, out AnnotatedMove move))
                return Move(move.Move);

            return ErrorCondition.InvalidInput;
        }

        public ErrorCondition Move(AnnotatedMove move) => Move(move.Move);

        private ErrorCondition Move(Move move)
        {
            if (!BitTranslator.IsValidSquare(move.StartFile, move.StartRank))
                return ErrorCondition.InvalidSquare;
            if (!BitTranslator.IsValidSquare(move.EndFile, move.EndRank))
                return ErrorCondition.InvalidSquare;

            ulong startSquare = BitTranslator.TranslateToBit(move.StartFile, move.StartRank);
            if ((CurrentTurn & startSquare) == 0)
                return ErrorCondition.MustMoveOwnPiece; // Can't move if not your turn

            ulong endSquare = BitTranslator.TranslateToBit(move.EndFile, move.EndRank);
            if ((CurrentTurn & endSquare) != 0)
                return ErrorCondition.CantTakeOwnPiece; // Can't end move on own piece

            ulong allMoves = MoveGenerator.GenerateValidMovesForPiece(CurrentState, startSquare);
            if ((endSquare & allMoves) == 0)
                return ErrorCondition.InvalidMovement; // End square is not a valid move

            // The move is good, so update state
            // Update current state
            UpdateState(move, startSquare, endSquare);

            return ErrorCondition.None;
        }

        public bool Undo()
        {
            if (History.Count <= 1)
                return false;

            History.Pop();
            CurrentState = History.Peek();
            CurrentTurn = (History.Count % 2 == 1) ? CurrentState.Board.WhitePieces : CurrentState.Board.BlackPieces;

            return true;
        }

        private void UpdateState(Move move, ulong startSquare, ulong endSquare)
        {
            var newState = GameStateMutator.ApplyMove(CurrentState, move, startSquare, endSquare);
            var opponentPieces = (endSquare & newState.Board.WhitePieces) != 0
                ? newState.Board.BlackPieces : newState.Board.WhitePieces;

            newState = AnalyzeAndApplyState(newState, endSquare, opponentPieces);

            CurrentState = newState;
            CurrentTurn = opponentPieces;

            History.Push(newState);
        }

        private static GameState AnalyzeAndApplyState(GameState newState, ulong endSquare, ulong opponentPieces)
        {
            // All state detections
            // ✔ Account for piece promotions
            // ✔ Detect checks
            // ✔ Detect checkmate
            // ✔ Detect stalemate
            // ✔ Detect draw by repetition
            // ✔ Detect draw by inactivity (50 moves without a capture)

            var newBoard = newState.Board;
            var didBlackMove = ((endSquare & newBoard.BlackPieces) != 0) ? 1 : 0;

            var whiteAttack = MoveGenerator.GenerateSquaresAttackedBy(newState, newBoard.WhitePieces);
            var blackAttack = MoveGenerator.GenerateSquaresAttackedBy(newState, newBoard.BlackPieces);

            // TODO: 'Squares attacked by pawns' here will only show if square is CURRENTLY occupied, rather than prospective attacks
            var squaresAttackedBy = new IndexedTuple<ulong>(whiteAttack, blackAttack);

            var ownMovements = squaresAttackedBy.Get(didBlackMove);
            var opponentMovements = MoveGenerator.GenerateValidMoves(newState, opponentPieces, ownMovements);

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
                var newStateUnmovedCastlingPieces = (newState.PiecesOnStartSquares & MoveGenerator.StartingKingsAndRooks);

                foreach (var state in newState.PossibleRepeatedHistory)
                {
                    count++;
                    if (BoardState.Equals(state.Item1, newBoard) && state.Item2 == newStateUnmovedCastlingPieces)
                        duplicateCount++;
                }

                if (count == Constants.MoveLimits.InactivityLimit)
                    attackState = AttackState.DrawByInactivity;
                else if (duplicateCount >= Constants.MoveLimits.RepetitionLimit)
                    attackState = AttackState.DrawByRepetition;
            }

            return newState.SetAttackState(attackState, squaresAttackedBy);
        }

        internal ErrorCondition Move(char startFile, int startRank, char endFile, int endRank)
        {
            return Move(new Move(startFile, startRank, endFile, endRank));
        }

        public List<Square> GetValidMoves(char file, int rank)
        {
            if (!BitTranslator.IsValidSquare(file, rank))
                throw new InvalidOperationException();

            var square = BitTranslator.TranslateToBit(file, rank);
            ulong allMoves = MoveGenerator.GenerateValidMovesForPiece(CurrentState, square);

            return BitTranslator.TranslateToSquares(allMoves);
        }

        public AnnotatedMove ParseMove(string input)
        {
            if (MoveParser.TryParseMove(input, CurrentState.Board, CurrentTurn, out AnnotatedMove move))
                return move;

            throw new FormatException($"Could not parse move '{input}' for current board state");
        }

        public static Square ParseSquare(string input)
        {
            return MoveParser.ParseSquare(input);
        }
    }
}
