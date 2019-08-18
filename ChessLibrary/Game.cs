using ChessLibrary.Models;
using System;
using System.Collections.Generic;

namespace ChessLibrary
{
    public class Game
    {
        private BoardState BoardState { get; set; }
        private ulong CurrentTurn { get; set; }

        public Game() : this(BoardState.DefaultPositions)
        {
        }

        internal Game(BoardState state)
        {
            BoardState = state;
            CurrentTurn = BoardState.WhitePieces;
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

        public ErrorConditions Move(string input)
        {
            if (MoveParser.TryParseMove(input, BoardState, CurrentTurn, out Move move))
                return Move(move.StartFile, move.StartRank, move.EndFile, move.EndRank);

            return ErrorConditions.InvalidInput;
        }

        public ErrorConditions Move(char startFile, int startRank, char endFile, int endRank)
        {
            if (!BitTranslator.IsValidSquare(startFile, startRank))
                return ErrorConditions.InvalidSquare;
            if (!BitTranslator.IsValidSquare(endFile, endRank))
                return ErrorConditions.InvalidSquare;

            ulong startSquare = BitTranslator.TranslateToBit(startFile, startRank);
            if ((CurrentTurn & startSquare) == 0)
                return ErrorConditions.MustMoveOwnPiece; // Can't move if not your turn

            ulong endSquare = BitTranslator.TranslateToBit(endFile, endRank);
            if ((CurrentTurn & endSquare) != 0)
                return ErrorConditions.CantTakeOwnPiece; // Can't end move on own piece

            ulong allMoves = MoveGenerator.GenerateMoves(BoardState, startSquare);
            if ((endSquare & allMoves) == 0)
                return ErrorConditions.InvalidMovement; // End square is not a valid move

            // The move is good, so update state
            // Update current state 
            BoardStateManipulator.MovePiece(BoardState, startSquare, endSquare);
            if ((endSquare & BoardState.WhitePieces) != 0)
                CurrentTurn = BoardState.BlackPieces;
            else
                CurrentTurn = BoardState.WhitePieces;

            return ErrorConditions.None;

            // TODO: Account for piece promotions
            // TODO: Detect checks
            // - via direct move
            // - via discovery
            // -- of piece moved
            // -- of piece captured (en passant)
            // TODO: Detect checkmate
        }

        public List<Square> GetValidMoves(char file, int rank)
        {
            if (!BitTranslator.IsValidSquare(file, rank))
                throw new InvalidOperationException();

            var square = BitTranslator.TranslateToBit(file, rank);
            ulong allMoves = MoveGenerator.GenerateMoves(BoardState, square);

            return BitTranslator.TranslateToSquares(allMoves);
        }

        public Move ParseMove(string input)
        {
            Move move;

            if (!MoveParser.TryParseMove(input, BoardState, CurrentTurn, out move))
                throw new FormatException($"Could not parse move '{input}' for current board state");

            return move;
        }
    }
}
