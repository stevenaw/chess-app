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
            // ❔ Account for descriptive marks (Na4+, e2#)
            // ❔ Account for good/bad marks (Na4!, e2??, b5!?)
            // ❔ Account for whitespace (Ne2 x a4) - fail in this case


            var trimmedInput = input.AsSpan().Trim();
            result = new Move();

            if (trimmedInput.IsEmpty)
                return false;


            var isWhiteMove = piecesOnCurrentSide == state.WhitePieces;

            var castleKingside = trimmedInput.Equals("0-0".AsSpan(), StringComparison.OrdinalIgnoreCase);
            var castleQueenside = trimmedInput.Equals("0-0-0".AsSpan(), StringComparison.OrdinalIgnoreCase);
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


            int squareIdx = 0;
            char pieceDesignation = '\0';
            if (PieceDesignations.Contains(input[squareIdx]))
                pieceDesignation = input[squareIdx++];

            char file = Char.ToLower(input[squareIdx++]);
            int rank = input[squareIdx++] - '0';

            if (squareIdx < input.Length && (
                input[squareIdx] == '-' || input[squareIdx] == 'x' || input[squareIdx] == ' '
                ))
            {
                result.StartFile = file;
                result.StartRank = rank;

                result.EndFile = Char.ToLower(input[++squareIdx]);
                result.EndRank = input[++squareIdx] - '0';

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

                    // TODO: Extrapolate start square for form like Na4
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
