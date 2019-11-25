using ChessLibrary.Models;
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

        public static ulong GenerateMovesForPiece(BoardState state, ulong square)
        {
            // Detect type of piece
            // Create bitmask of all moves for that piece
            // ✔ Account for can't end in check
            //   ✔ Account for pins
            //   ✔ Account for can't move king into check
            // ❔ Account for can't castle through check

            var isWhite = (square & state.WhitePieces) != 0;
            var opposingPiecesCurrent = isWhite ? state.BlackPieces : state.WhitePieces;
            var opponentMoves = GenerateStandardMoves(state, opposingPiecesCurrent, 0);

            return GenerateMovesForPiece(state, square, opponentMoves);
        }

        public static ulong GenerateMovesForPiece(BoardState state, ulong square, ulong opposingMoves)
        {
            // Detect type of piece
            // Create bitmask of all moves for that piece
            // ✔ Account for can't end in check
            //   ✔ Account for pins
            //   ✔ Account for can't move king into check
            // ❔ Account for can't castle through check

            var isWhite = (square & state.WhitePieces) != 0;

            var ownPiecesCurrent = isWhite ? state.WhitePieces : state.BlackPieces;
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

        private static bool WillMovePlaceKingUnderAttack(BoardState state, ulong square, ulong targetMove)
        {
            // Try the move and see if it lands in check
            // There's probably a better way to do this using a stateful 'squares attacked by' approach

            var newState = state.Copy().MovePiece(square, targetMove);

            var ownPieces = (newState.WhitePieces & targetMove) != 0 ? newState.WhitePieces : newState.BlackPieces;
            var opposingPieces = newState.AllPieces & ~ownPieces;
            var opposingAttack = GenerateStandardMoves(newState, opposingPieces, 0);
            var isKingUnderAttack = opposingAttack & (ownPieces & newState.Kings);

            return isKingUnderAttack != 0;
        }

        public static ulong GenerateMoves(BoardState state, ulong squareMask, ulong opponentMoves)
        {
            ulong result = 0;

            for (var i = 0; i < 64; i++)
            {
                var targetBit = squareMask & (1UL << i);
                var piece = state.AllPieces & targetBit;
                if (piece != 0)
                {
                    var attackingSquares = GenerateMovesForPiece(state, piece, opponentMoves);
                    result |= attackingSquares;
                }
            }

            return result;
        }

        private static ulong GenerateStandardMovesForPiece(BoardState state, ulong square, ulong opponentMoves)
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
                var moves = GetKingMovements(square) & ~opponentMoves;
                result |= moves;
            }

            return result;
        }

        public static ulong GenerateStandardMoves(BoardState state, ulong squareMask, ulong opponentMoves)
        {
            ulong result = 0;

            for (var i = 0; i < 64; i++)
            {
                var targetBit = squareMask & (1UL << i);
                var piece = state.AllPieces & targetBit;
                if (piece != 0)
                {
                    var attackingSquares = GenerateStandardMovesForPiece(state, piece, opponentMoves);
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

            return newSquares;
        }

        private static ulong FillDiagonals(ulong input, BoardState state)
        {
            // TODO: FIX. Passing ShiftRight or ShiftLeft allocates
            ulong descendingDiagonal = TraverseUntilCant(state, input, ShiftRight, 8 - 1, FileA | Rank1)
                | TraverseUntilCant(state, input, ShiftLeft, 8 + 1, FileH | Rank8);

            ulong ascendingDiagonal = TraverseUntilCant(state, input, ShiftRight, 8 + 1, FileH | Rank1)
                | TraverseUntilCant(state, input, ShiftLeft, 8 - 1, FileA | Rank8);

            return ascendingDiagonal | descendingDiagonal;
        }

        private static ulong FillVerticalAndHorizontal(ulong input, BoardState state)
        {
            // TODO: FIX. Passing ShiftRight or ShiftLeft allocates
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
