﻿using ChessLibrary.Models;
using System;

namespace ChessLibrary
{
    internal static class MoveGenerator
    {
        public const ulong Rank1 = 0x00000000000000FF;
        public const ulong Rank8 = 0xFF00000000000000;
        public const ulong FileA = 0x0101010101010101;
        public const ulong FileAB = 0x0303030303030303;
        public const ulong FileH = 0x8080808080808080;
        public const ulong FileGH = 0xC0C0C0C0C0C0C0C0;
        public const ulong AllRanksFiles = 0xFFFFFFFFFFFFFFFF;

        public const ulong PawnStartRowWhite = 0x000000000000FF00;
        public const ulong PawnStartRowBlack = 0x00FF000000000000;

        public static ulong GenerateMovesForPiece(GameState state, ulong square)
        {
            var board = state.Board;
            var isWhite = (square & board.WhitePieces) != 0;
            var opposingPiecesCurrent = isWhite ? board.BlackPieces : board.WhitePieces;
            var opponentMoves = GenerateStandardMoves(state, opposingPiecesCurrent, 0);

            return GenerateMovesForPiece(state, square, opponentMoves);
        }

        public static ulong GenerateMovesForPiece(GameState state, ulong square, ulong opposingMoves)
        {
            var board = state.Board;
            var isWhite = (square & board.WhitePieces) != 0;

            var ownPiecesCurrent = isWhite ? board.WhitePieces : board.BlackPieces;
            var moves = GenerateStandardMovesForPiece(state, square, opposingMoves);
            moves &= ~ownPiecesCurrent;

            var validMoves = 0UL;
            for (var i = 0; i < 64; i++)
            {
                var targetMove = moves & (1UL << i);
                if (targetMove != 0)
                {
                    var isKingUnderAttack = WillMovePlaceKingUnderAttack(state, square, targetMove);
                    if (!isKingUnderAttack)
                        validMoves |= targetMove;
                }
            }

            return validMoves;
        }

        private static bool WillMovePlaceKingUnderAttack(GameState state, ulong square, ulong targetMove)
        {
            // Try the move and see if it lands in check
            // There's probably a better way to do this using a stateful 'squares attacked by' approach
            // TODO: Do this better
            var newState = GameStateMutator.ApplyMove(state, square, targetMove);
            var newBoard = newState.Board;

            var ownPieces = (newBoard.WhitePieces & targetMove) != 0 ? newBoard.WhitePieces : newBoard.BlackPieces;
            var opposingPieces = newBoard.AllPieces & ~ownPieces;
            var opposingAttack = GenerateStandardMoves(newState, opposingPieces, 0);
            var isKingUnderAttack = opposingAttack & (ownPieces & newBoard.Kings);

            return isKingUnderAttack != 0;
        }

        public static ulong GenerateMoves(GameState state, ulong squareMask, ulong opponentMoves)
        {
            ulong result = 0;

            for (var i = 0; i < 64; i++)
            {
                var targetBit = squareMask & (1UL << i);
                var piece = state.Board.AllPieces & targetBit;
                if (piece != 0)
                {
                    var attackingSquares = GenerateMovesForPiece(state, piece, opponentMoves);
                    result |= attackingSquares;
                }
            }

            return result;
        }

        private static ulong GenerateStandardMovesForPiece(GameState state, ulong square, ulong opponentMoves)
        {
            // Detect type of piece
            // Create bitmask of all moves for that piece
            // ✔ Detect if pieces in way (unless knight is moving)
            // ✔ Detect if moving onto own piece
            // ✔ Detect standard patterns of movement
            // ✔ Account for en passant
            // ❔ Account for castling
            //   ❔ Account for can't castle through check
            //   ❔ Account for can't castle while in check

            ulong result = 0;
            var board = state.Board;

            if ((square & board.Queens) != 0)
            {
                result |= GetQueenMovements(square, board);
            }
            else if ((square & board.Rooks) != 0)
            {
                result |= GetRookMovements(square, board);
            }
            else if ((square & board.Bishops) != 0)
            {
                result |= GetBishopMovements(square, board);
            }
            else if ((square & board.Pawns) != 0)
            {
                result |= GetPawnMovements(square, board, state.PrecedingMove);
            }
            else if ((square & board.Knights) != 0)
            {
                result |= GetKnightMovements(square);
            }
            else if ((square & board.Kings) != 0)
            {
                var moves = GetKingMovements(square) & ~opponentMoves;
                result |= moves;
            }

            return result;
        }

        public static ulong GenerateStandardMoves(GameState state, ulong squareMask, ulong opponentMoves)
        {
            ulong result = 0;

            for (var i = 0; i < 64; i++)
            {
                var targetBit = squareMask & (1UL << i);
                var piece = state.Board.AllPieces & targetBit;
                if (piece != 0)
                {
                    var attackingSquares = GenerateStandardMovesForPiece(state, piece, opponentMoves);
                    result |= attackingSquares;
                }
            }

            return result;
        }

        private static readonly Func<ulong, int, ulong> ShiftLeft = (ulong lvalue, int rvalue) =>
        {
            return lvalue << rvalue;
        };
        private static readonly Func<ulong, int, ulong> ShiftRight = (ulong lvalue, int rvalue) =>
        {
            return lvalue >> rvalue;
        };

        internal static ulong GetQueenMovements(ulong input, BoardState state)
        {
            return FillVerticalAndHorizontal(input, state) | FillDiagonals(input, state);
        }

        internal static ulong GetBishopMovements(ulong input, BoardState state)
        {
            return FillDiagonals(input, state);
        }

        internal static ulong GetRookMovements(ulong input, BoardState state)
        {
            return FillVerticalAndHorizontal(input, state);
        }

        internal static ulong GetKingMovements(ulong input)
        {
            ulong newSquares = 0;

            // Moving west
            newSquares |= (input << 7) & ~FileH;
            newSquares |= (input >> 1) & ~FileH;
            newSquares |= (input >> 9) & ~FileH;

            // Moving east
            newSquares |= (input << 9) & ~FileA;
            newSquares |= (input << 1) & ~FileA;
            newSquares |= (input >> 7) & ~FileA;

            // Moving north/south
            newSquares |= (input >> 8);
            newSquares |= (input << 8);

            return newSquares;
        }

        internal static ulong GetKnightMovements(ulong input)
        {
            ulong newSquares = 0;

            newSquares |= (input << 17) & ~FileA;
            newSquares |= (input << 15) & ~FileH;
            newSquares |= (input << 10) & ~FileAB;
            newSquares |= (input <<  6) & ~FileGH;

            newSquares |= (input >> 17) & ~FileH;
            newSquares |= (input >> 15) & ~FileA;
            newSquares |= (input >> 10) & ~FileGH;
            newSquares |= (input >>  6) & ~FileAB;

            return newSquares;
        }

        private static ulong GetPawnMovements(ulong input, BoardState state, Move previousMove)
        {
            ulong newSquares = 0;
            var isWhite = (input & state.WhitePieces) != 0;

            if (isWhite)
            {
                // straight ahead
                newSquares = ShiftLeft(input, 8) & ~state.AllPieces;

                // up 2 on first move
                if (newSquares != 0 && (input & PawnStartRowWhite) != 0)
                    newSquares |= (ShiftLeft(newSquares, 8) & ~state.AllPieces);

                var attackSquares = (ShiftLeft(input, 7) & ~FileH)
                    | (ShiftLeft(input, 9) & ~FileA);
                newSquares |= (attackSquares & state.AllPieces);
            }
            else
            {
                newSquares = ShiftRight(input, 8) & ~state.AllPieces;
                if (newSquares != 0 && (input & PawnStartRowBlack) != 0)
                    newSquares |= (ShiftRight(newSquares, 8) & ~state.AllPieces);

                var attackSquares = (ShiftRight(input, 7) & ~FileA)
                    | (ShiftRight(input, 9) & ~FileH);
                newSquares |= (attackSquares & state.AllPieces);
            }

            // Check for en passant
            if (!Move.Equals(previousMove, Move.Empty))
            {
                var squareForGeneration = BitTranslator.TranslateToSquare(input);

                // Previous move ended right next to this piece
                if (squareForGeneration.Rank == previousMove.EndRank
                    && Math.Abs((int)squareForGeneration.File - (int)previousMove.EndFile) == 1)
                {
                    // Previous move was also a pawn which had moved up 2
                    var lastMoveEndSquare = BitTranslator.TranslateToBit(previousMove.EndFile, previousMove.EndRank);
                    if ((state.Pawns & lastMoveEndSquare) != 0
                        && Math.Abs((int)previousMove.StartRank - (int)previousMove.EndRank) == 2)
                    {
                        // TODO: Probably a better way to do all this
                        var captureRow = isWhite ? ShiftLeft(input, 8) : ShiftRight(input, 8);
                        var shifted = previousMove.EndFile > squareForGeneration.File ? ShiftLeft(captureRow, 1) : ShiftRight(captureRow, 1);
                        newSquares |= shifted;
                    }
                }
            }

            return newSquares;
        }

        private static ulong FillDiagonals(ulong input, BoardState state)
        {
            ulong descendingDiagonal = TraverseUntilCant(state, input, ShiftRight, 8 - 1, FileA | Rank1)
                | TraverseUntilCant(state, input, ShiftLeft, 8 + 1, FileH | Rank8);

            ulong ascendingDiagonal = TraverseUntilCant(state, input, ShiftRight, 8 + 1, FileH | Rank1)
                | TraverseUntilCant(state, input, ShiftLeft, 8 - 1, FileA | Rank8);

            return ascendingDiagonal | descendingDiagonal;
        }

        private static ulong FillVerticalAndHorizontal(ulong input, BoardState state)
        {
            ulong vertical = TraverseUntilCant(state, input, ShiftRight, 8, Rank1)
                | TraverseUntilCant(state, input, ShiftLeft, 8, Rank8);

            ulong horizontal = TraverseUntilCant(state, input, ShiftRight, 1, FileA)
                | TraverseUntilCant(state, input, ShiftLeft, 1, FileH);

            return vertical | horizontal;
        }

        private static ulong TraverseUntilCant
        (
            BoardState state,
            ulong input,
            Func<ulong, int, ulong> shift,
            int stepSize,
            ulong border
        )
        {
            ulong result = 0;
            int walkSize = stepSize;
            var stopCondition = border | state.AllPieces;

            if ((input & border) != 0)
                return result;

            while ((result & stopCondition) == 0)
            {
                var possibleMove = shift(input, walkSize);
                result |= possibleMove;
                walkSize += stepSize;
            }

            return result;
        }
    }
}
