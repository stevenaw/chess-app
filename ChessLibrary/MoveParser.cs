using ChessLibrary.Models;
using System;
using System.Linq;

namespace ChessLibrary
{
    internal static class MoveParser
    {
        private static readonly char[] PieceDesignations = new char[] {
            'P',
            'N',
            'B',
            'R',
            'Q',
            'K'
        };

        // Parses a move, but does not determine validity
        public static bool TryParseMove(string input, BoardState state, ulong piecesOnCurrentSide, out Move result)
        {
            // TODO: Properly parse move + return
            // ✔ Account for castling (0-0, 0-0-0)
            // ✔ Account for regular move (a4, Ng8)
            // ✔ Account for long-form move (Nb1 c3, Nb1-c3)
            // ✔ Account for long-form capture (Ne2xa4)
            // ❔ Account for short-form capture (Nxg4, axb4)
            // ❔ Account for disambiguation (Ngg4)
            // ✔ Account for piece promotions (e8=Q)
            //   ❔ Account for if pawn moves to end without speccifying promotion
            // ✔ Account for state change marks (Na4+, e2#)
            // ✔ Account for annotations (Na4!, e2??, b5!?)
            // ❔ Account for whitespace (Ne2 x a4) - fail in this case


            var trimmedInput = input.AsSpan().Trim();
            result = new Move();

            if (trimmedInput.IsEmpty)
                return false;


            var isWhiteMove = piecesOnCurrentSide == state.WhitePieces;

            var moveNotation = TrimMoveDescriptors(trimmedInput);
            var moveDescriptors = moveNotation.IsEmpty || moveNotation.Length == trimmedInput.Length
                ? ReadOnlySpan<char>.Empty
                : trimmedInput.Slice(moveNotation.Length);

            result.Annotation = GetAnnotation(moveDescriptors);
            result.AttackState = GetAttackState(moveDescriptors);

            // Handle castling
            if (TryHandleCastling(moveNotation, isWhiteMove, ref result))
                return true;

            // TODO: Change this so it parses from end first (always have end square, infer start square any any other things)
            int squareIdx = 0;
            char pieceDesignation = '\0';
            if (PieceDesignations.Contains(moveNotation[squareIdx]))
                pieceDesignation = moveNotation[squareIdx++];

            // TODO: Sanity check for if at end of board too
            if (pieceDesignation == '\0' || pieceDesignation == 'P')
                result.PromotedPiece = GetPromotion(moveDescriptors, isWhiteMove);

            char file = Char.ToLower(moveNotation[squareIdx++]);
            int rank = moveNotation[squareIdx++] - '0';

            if (squareIdx < moveNotation.Length && (
                moveNotation[squareIdx] == '-' || moveNotation[squareIdx] == 'x' || moveNotation[squareIdx] == ' '
                ))
            {
                result.StartFile = file;
                result.StartRank = rank;

                result.EndFile = Char.ToLower(moveNotation[++squareIdx]);
                result.EndRank = moveNotation[++squareIdx] - '0';

                return true;
            }
            else
            {
                result.EndFile = file;
                result.EndRank = rank;

                var endBit = BitTranslator.TranslateToBit(file, rank);

                switch(pieceDesignation)
                {
                    case 'K':
                        {
                            var possibleStartBits = MoveGenerator.GetKingMovements(endBit);
                            var existingOfPiece = state.Kings & piecesOnCurrentSide;
                            var actualStartPiece = possibleStartBits & existingOfPiece;

                            if (actualStartPiece == 0)
                                return false;

                            var startSquare = BitTranslator.TranslateToSquare(actualStartPiece);

                            result.StartFile = startSquare.File;
                            result.StartRank = startSquare.Rank;
                            return true;
                        }

                    case 'N':
                        {
                            var possibleStartBits = MoveGenerator.GetKnightMovements(endBit);
                            var existingOfPiece = state.Knights & piecesOnCurrentSide;
                            var actualStartPiece = possibleStartBits & existingOfPiece;

                            if (actualStartPiece == 0)
                                return false;

                            var startSquare = BitTranslator.TranslateToSquare(actualStartPiece);

                            result.StartFile = startSquare.File;
                            result.StartRank = startSquare.Rank;
                            return true;
                        }

                    case 'Q':
                        {
                            var possibleStartBits = MoveGenerator.GetQueenMovements(endBit, state);
                            var existingOfPiece = state.Queens & piecesOnCurrentSide;
                            var actualStartPiece = possibleStartBits & existingOfPiece;

                            if (actualStartPiece == 0)
                                return false;

                            var startSquare = BitTranslator.TranslateToSquare(actualStartPiece);

                            result.StartFile = startSquare.File;
                            result.StartRank = startSquare.Rank;
                            return true;
                        }

                    case 'R':
                        {
                            var possibleStartBits = MoveGenerator.GetRookMovements(endBit, state);
                            var existingOfPiece = state.Rooks & piecesOnCurrentSide;
                            var actualStartPiece = possibleStartBits & existingOfPiece;

                            if (actualStartPiece == 0)
                                return false;

                            var startSquare = BitTranslator.TranslateToSquare(actualStartPiece);

                            result.StartFile = startSquare.File;
                            result.StartRank = startSquare.Rank;
                            return true;
                        }

                    case 'B':
                        {
                            var possibleStartBits = MoveGenerator.GetBishopMovements(endBit, state);
                            var existingOfPiece = state.Bishops & piecesOnCurrentSide;
                            var actualStartPiece = possibleStartBits & existingOfPiece;

                            if (actualStartPiece == 0)
                                return false;

                            var startSquare = BitTranslator.TranslateToSquare(actualStartPiece);

                            result.StartFile = startSquare.File;
                            result.StartRank = startSquare.Rank;
                            return true;
                        }

                    case '\0':
                    case 'P':
                        {
                            ulong actualStartPiece = 0;

                            if (isWhiteMove)
                            {
                                var possibleStart = endBit >> 8 | endBit >> 16;
                                var pawns = state.Pawns & state.WhitePieces;
                                actualStartPiece = pawns & possibleStart;
                            }
                            else
                            {
                                var possibleStart = endBit << 8 | endBit << 16;
                                var pawns = state.Pawns & state.BlackPieces;
                                actualStartPiece = pawns & possibleStart;
                            }

                            if (actualStartPiece == 0)
                                return false;

                            var startSquare = BitTranslator.TranslateToSquare(actualStartPiece);

                            result.StartFile = startSquare.File;
                            result.StartRank = startSquare.Rank;
                            return true;
                        }

                    default:
                        return false;
                }
            }
        }

        private static ReadOnlySpan<char> TrimMoveDescriptors(ReadOnlySpan<char> move)
        {
            for (var i = move.Length - 1; i >= 0; i--)
                if (Char.IsDigit(move[i]))
                    return move.Slice(0, i + 1);

            return ReadOnlySpan<char>.Empty;
        }

        private static bool TryHandleCastling(ReadOnlySpan<char> moveNotation, bool isWhiteMove, ref Move result)
        {
            var castleKingside = moveNotation.Equals("0-0".AsSpan(), StringComparison.OrdinalIgnoreCase);
            var castleQueenside = moveNotation.Equals("0-0-0".AsSpan(), StringComparison.OrdinalIgnoreCase);

            if (castleKingside || castleQueenside)
            {
                result.StartFile = 'e';
                result.EndFile = castleKingside ? 'g' : 'c';

                if (isWhiteMove)
                    result.StartRank = result.EndRank = 1;
                else
                    result.StartRank = result.EndRank = 8;

                return true;
            }

            return false;
        }

        private static MoveAnnotation GetAnnotation(ReadOnlySpan<char> descriptor)
        {
            if (descriptor.IsEmpty)
                return MoveAnnotation.Normal;
            else if (descriptor[0] == '!')
            {
                if (descriptor.Length == 1)
                    return MoveAnnotation.Excellent;
                else if (descriptor[1] == '!')
                    return MoveAnnotation.Brilliancy;
                else if (descriptor[1] == '?')
                    return MoveAnnotation.Interesting;

                throw new InvalidOperationException("Unknown move annotation");
            }
            else if (descriptor[0] == '?')
            {
                if (descriptor.Length == 1)
                    return MoveAnnotation.Mistake;
                else if (descriptor[1] == '?')
                    return MoveAnnotation.Blunder;
                else if (descriptor[1] == '!')
                    return MoveAnnotation.Dubious;

                throw new InvalidOperationException("Unknown move annotation");
            }

            return MoveAnnotation.Normal;
        }
        private static AttackState GetAttackState(ReadOnlySpan<char> descriptor)
        {
            if (descriptor.Length == 1)
            {
                if (descriptor[0] == '+')
                    return AttackState.Check;
                else if (descriptor[0] == '#')
                    return AttackState.Checkmate;
            }

            return AttackState.None;
        }

        private static SquareContents GetPromotion(ReadOnlySpan<char> descriptor, bool isWhiteTurn)
        {
            if (descriptor.Length == 2 && descriptor[0] == '=')
            {
                var contents = GetSquareContents(descriptor[1], isWhiteTurn);

                // Invalid to promote to pawn. Should this throw?
                if ((contents & SquareContents.Pawn) != 0)
                    return SquareContents.Empty;

                return contents;
            }

            return SquareContents.Empty;
        }

        public static SquareContents GetSquareContents(char piece, bool isWhiteTurn)
        {
            SquareContents side = isWhiteTurn ? SquareContents.White : SquareContents.Black;
            switch (piece)
            {
                case 'K':
                    return SquareContents.King | side;
                case 'Q':
                    return SquareContents.Queen | side;
                case 'R':
                    return SquareContents.Rook | side;
                case 'B':
                    return SquareContents.Bishop | side;
                case 'N':
                    return SquareContents.Knight | side;
                case 'P':
                    return SquareContents.Pawn | side;
            }

            return SquareContents.Empty;
        }

        // TODO: Move this + below to a different class?
        public static Square ParseSquare(string input)
        {
            var result = new Square();

            result.File = Char.ToLower(input[0]);
            result.Rank = input[1] - '0';

            return result;
        }
    }
}
