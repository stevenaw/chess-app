﻿using ChessLibrary.Models;
using System;
using System.Collections.Generic;

namespace ChessLibrary
{
    public sealed class Game
    {
        internal GameState CurrentState { get; private set; }
        private Stack<GameState> GameHistory { get; } = new Stack<GameState>();

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
                ulong startSquare = BitTranslator.TranslateToBit(file, rank);

                if ((startSquare & CurrentState.Board.WhitePieces) != 0)
                    result |= SquareContents.White;
                else if ((startSquare & CurrentState.Board.BlackPieces) != 0)
                    result |= SquareContents.Black;

                if ((startSquare & CurrentState.Board.Kings) != 0)
                    result |= SquareContents.King;
                else if ((startSquare & CurrentState.Board.Queens) != 0)
                    result |= SquareContents.Queen;
                else if ((startSquare & CurrentState.Board.Rooks) != 0)
                    result |= SquareContents.Rook;
                else if ((startSquare & CurrentState.Board.Bishops) != 0)
                    result |= SquareContents.Bishop;
                else if ((startSquare & CurrentState.Board.Knights) != 0)
                    result |= SquareContents.Knight;
                else if ((startSquare & CurrentState.Board.Pawns) != 0)
                    result |= SquareContents.Pawn;
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

            ulong allMoves = MoveGenerator.GenerateMovesForPiece(CurrentState, startSquare);
            if ((endSquare & allMoves) == 0)
                return ErrorCondition.InvalidMovement; // End square is not a valid move

            // The move is good, so update state
            // Update current state
            UpdateState(move, startSquare, endSquare);

            return ErrorCondition.None;
        }

        private void UpdateState(Move move, ulong startSquare, ulong endSquare)
        {
            var newState = GameStateMutator.ApplyMove(CurrentState, move, startSquare, endSquare);
            var opponentPieces = (endSquare & newState.Board.WhitePieces) != 0
                ? newState.Board.BlackPieces : newState.Board.WhitePieces;

            newState = AnalyzeAndApplyState(newState, endSquare, opponentPieces);

            CurrentState = newState;
            GameHistory.Push(newState);
            CurrentTurn = opponentPieces;
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

            var updatedKingMoveStatus = new IndexedTuple<bool>(
                newState.HasKingMoved.First || ((newBoard.WhitePieces & newBoard.Kings & endSquare) != 0),
                newState.HasKingMoved.Second || ((newBoard.BlackPieces & newBoard.Kings & endSquare) != 0)
            );

            return newState.SetAttackState(attackState, updatedKingMoveStatus);
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
            ulong allMoves = MoveGenerator.GenerateMovesForPiece(CurrentState, square);

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
