using ChessLibrary.Models;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace ChessLibrary
{
    public class Game
    {
        // TODO: ImmutableDictionary<BoardState, int>  ??
        private ImmutableStack<BoardState> History { get; set; } = ImmutableStack<BoardState>.Empty;
        private Stack<GameState> GameHistory { get; } = new Stack<GameState>();

        private GameState GameState { get; set; }
        private ulong CurrentTurn { get; set; }
        private BoardState BoardState { get { return GameState.BoardState; } }
        public AttackState AttackState { get { return GameState.AttackState; } }

        public Game() : this(BoardState.DefaultPositions, PieceColor.White)
        { }

        internal Game(BoardState state) : this(state, PieceColor.White)
        { }

        internal Game(BoardState state, PieceColor turn)
        {
            GameState = GameState.Initialize(state);
            CurrentTurn = turn == PieceColor.White ? BoardState.WhitePieces : BoardState.BlackPieces;
        }

        public PieceColor GetTurn()
        {
            return (CurrentTurn & BoardState.WhitePieces) != 0
                ? PieceColor.White
                : PieceColor.Black;
        }

        public SquareContents GetSquareContents(char file, int rank)
        {
            var result = (SquareContents)0;

            if (BitTranslator.IsValidSquare(file, rank))
            {
                ulong startSquare = BitTranslator.TranslateToBit(file, rank);

                if ((startSquare & BoardState.WhitePieces) != 0)
                    result |= SquareContents.White;
                else if ((startSquare & BoardState.BlackPieces) != 0)
                    result |= SquareContents.Black;

                if ((startSquare & BoardState.Kings) != 0)
                    result |= SquareContents.King;
                else if ((startSquare & BoardState.Queens) != 0)
                    result |= SquareContents.Queen;
                else if ((startSquare & BoardState.Rooks) != 0)
                    result |= SquareContents.Rook;
                else if ((startSquare & BoardState.Bishops) != 0)
                    result |= SquareContents.Bishop;
                else if ((startSquare & BoardState.Knights) != 0)
                    result |= SquareContents.Knight;
                else if ((startSquare & BoardState.Pawns) != 0)
                    result |= SquareContents.Pawn;
            }

            return result;
        }

        public ErrorCondition Move(string input)
        {
            if (MoveParser.TryParseMove(input, BoardState, CurrentTurn, out Move move))
                return Move(move);

            return ErrorCondition.InvalidInput;
        }

        public ErrorCondition Move(Move move)
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

            ulong allMoves = MoveGenerator.GenerateMovesForPiece(GameState, startSquare);
            if ((endSquare & allMoves) == 0)
                return ErrorCondition.InvalidMovement; // End square is not a valid move

            // The move is good, so update state
            // Update current state
            UpdateState(move, startSquare, endSquare);

            return ErrorCondition.None;
        }

        private void UpdateState(Move move, ulong startSquare, ulong endSquare)
        {
            // All state detections
            // ✔ Account for piece promotions
            // ✔ Detect checks
            // ✔ Detect checkmate
            // ✔ Detect stalemate
            // ✔ Detect draw by repetition
            // ✔ Detect draw by inactivity (50 moves without a capture)

            var isCapture = (BoardState.AllPieces & endSquare) != 0; // TODO: Account for en passant
            var isPawn = (BoardState.Pawns & startSquare) != 0;

            var newState = GameStateMutator.ApplyMove(GameState, move);
            var newBoardState = newState.BoardState;

            if (isCapture || isPawn)
                History = History.Clear();

            History = History.Push(newBoardState);


            var ownPieces = (endSquare & newBoardState.WhitePieces) != 0
                ? newBoardState.WhitePieces : newBoardState.BlackPieces;
            var opponentPieces = newBoardState.AllPieces & ~ownPieces;

            var ownMovements = MoveGenerator.GenerateStandardMoves(newState, ownPieces, 0);
            var opponentMovements = MoveGenerator.GenerateMoves(newState, opponentPieces, ownMovements);

            var opponentKingUnderAttack = (opponentPieces & newBoardState.Kings & ownMovements) != 0;
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

                // TODO: Enumerating the whole stack can be slow
                foreach (var state in History)
                {
                    count++;
                    if (state.Equals(newBoardState))
                        duplicateCount++;
                }

                if (count == Constants.MoveLimits.InactivityLimit)
                    attackState = AttackState.DrawByInactivity;
                else if (duplicateCount >= Constants.MoveLimits.RepetitionLimit)
                    attackState = AttackState.DrawByRepetition;
            }

            

            move.AttackState = attackState;
            newState.SetAttackState(attackState);

            GameState = newState;
            GameHistory.Push(newState);

            CurrentTurn = opponentPieces;
        }

        internal ErrorCondition Move(char startFile, int startRank, char endFile, int endRank)
        {
            return Move(new Move()
            {
                StartFile = startFile,
                StartRank = startRank,
                EndFile = endFile,
                EndRank = endRank
            });
        }

        public List<Square> GetValidMoves(char file, int rank)
        {
            if (!BitTranslator.IsValidSquare(file, rank))
                throw new InvalidOperationException();

            var square = BitTranslator.TranslateToBit(file, rank);
            ulong allMoves = MoveGenerator.GenerateMovesForPiece(GameState, square);

            return BitTranslator.TranslateToSquares(allMoves);
        }

        public Move ParseMove(string input)
        {
            if (MoveParser.TryParseMove(input, BoardState, CurrentTurn, out Move move))
                return move;

            throw new FormatException($"Could not parse move '{input}' for current board state");
        }

        public static Square ParseSquare(string input)
        {
            return MoveParser.ParseSquare(input);
        }
    }
}
