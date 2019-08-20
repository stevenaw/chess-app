using ChessLibrary.Models;
using System;

namespace ChessLibrary
{
    internal static class MoveGenerator
    {
        private const ulong Rank1 = 0x00000000000000FF;
        private const ulong Rank8 = 0xFF00000000000000;
        private const ulong FileA = 0x0101010101010101;
        private const ulong FileAB = 0x0303030303030303;
        private const ulong FileH = 0x8080808080808080;
        private const ulong FileGH = 0xC0C0C0C0C0C0C0C0;

        private const ulong PawnStartRowWhite = 0x000000000000FF00;
        private const ulong PawnStartRowBlack = 0x00FF000000000000;

        public static ulong GenerateMovesForPiece(BoardState state, ulong square)
        {
            // Detect type of piece
            // Create bitmask of all moves for that piece
            // ✔ Account for can't end in check
            //   ✔ Account for pins
            //   ✔ Account for can't move king into check
            // ❔ Account for can't castle through check

            var isWhite = (square & state.WhitePieces) != 0;
            var moves = GenerateStandardMovesForPiece(state, square);
            var validMoves = 0UL;

            for(var i = 0; i < 64; i++)
            {
                var targetMove = moves & (1UL << i);
                if (targetMove != 0)
                {
                    // Try the move and see if it lands in check
                    var newState = new BoardState(state);
                    BoardStateManipulator.MovePiece(newState, square, targetMove);

                    var ownPieces = isWhite ? newState.WhitePieces : newState.BlackPieces;
                    var opposingPieces = newState.AllPieces & ~ownPieces;

                    var opposingAttack = GenerateAttackingSquares(newState, opposingPieces);
                    var isKingUnderAttack = opposingAttack & (ownPieces & newState.Kings);

                    if (isKingUnderAttack == 0)
                        validMoves |= targetMove;
                }
            }

            return validMoves;
        }
        private static ulong GenerateStandardMovesForPiece(BoardState state, ulong square)
        {
            // Detect type of piece
            // Create bitmask of all moves for that piece
            // ✔ Detect if pieces in way (unless knight is moving)
            // ✔ Detect if moving onto own piece
            // ✔ Detect standard patterns of movement
            // ❔ Account for en passant
            // ❔ Account for castling

            ulong result = 0;

            if ((square & state.Queens) != 0)
            {
                result |= GetQueenMovements(square, state);
            }
            else if ((square & state.Rooks) != 0)
            {
                result |= GetRookMovements(square, state);
            }
            else if ((square & state.Bishops) != 0)
            {
                result |= GetBishopMovements(square, state);
            }
            else if ((square & state.Pawns) != 0)
            {
                result |= GetPawnMovements(square, state);
            }
            else if ((square & state.Knights) != 0)
            {
                result |= GetKnightMovements(square);
            }
            else if ((square & state.Kings) != 0)
            {
                result |= GetKingMovements(square);
            }

            if ((square & state.WhitePieces) != 0)
                return result & ~state.WhitePieces;
            else
                return result & ~state.BlackPieces;
        }

        public static ulong GenerateAttackingSquares(BoardState state, ulong squareMask)
        {
            ulong result = 0;

            for (var i = 0; i < 64; i++)
            {
                var targetBit = squareMask & (1UL << i);
                var piece = state.AllPieces & targetBit;
                if (piece != 0)
                {
                    var attackingSquares = GenerateStandardMovesForPiece(state, piece);
                    result |= attackingSquares;
                }
            }

            return result;
        }

        private static ulong ShiftLeft(ulong lvalue, int rvalue)
        {
            return lvalue << rvalue;
        }

        private static ulong ShiftRight(ulong lvalue, int rvalue)
        {
            return lvalue >> rvalue;
        }

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

        internal static ulong GetPawnMovements(ulong input, BoardState state)
        {
            ulong newSquares = 0;

            if ((input & state.WhitePieces) != 0)
            {
                newSquares = ShiftLeft(input, 8) & ~state.AllPieces;
                if (newSquares != 0 && (input & PawnStartRowWhite) != 0)
                    newSquares |= (ShiftLeft(newSquares, 8) & ~state.AllPieces);

                var attackSquares = (ShiftLeft(input, 7) & ~FileH)
                    | (ShiftLeft(input, 9) & ~FileA);
                newSquares |= (attackSquares & state.BlackPieces);
            }
            else
            {
                newSquares = ShiftRight(input, 8) & ~state.AllPieces;
                if (newSquares != 0 && (input & PawnStartRowBlack) != 0)
                    newSquares |= (ShiftRight(newSquares, 8) & ~state.AllPieces);

                var attackSquares = (ShiftRight(input, 7) & ~FileA)
                    | (ShiftRight(input, 9) & ~FileH);
                newSquares |= (attackSquares & state.WhitePieces);
            }

            return newSquares;
        }

        private static ulong FillDiagonals(ulong input, BoardState state)
        {
            ulong ascendingDiagonal = TraverseUntilCant(state, input, ShiftRight, 8 - 1, FileA | Rank1)
                | TraverseUntilCant(state, input, ShiftLeft, 8 + 1, FileH | Rank8);

            ulong descendingDiagonal = TraverseUntilCant(state, input, ShiftRight, 8 + 1, FileH | Rank1)
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

            while ((result & (border | state.AllPieces)) == 0)
            {
                result |= shift(input, walkSize);
                walkSize += stepSize;
            }

            return result;
        }
    }
}
