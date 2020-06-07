using ChessLibrary.Models;
using System;
using System.Linq;

namespace ChessLibrary
{
    internal static class MoveParser
    {
        private static readonly char[] PieceDesignations = new char[] {
            Constants.PieceNotation.Pawn,
            Constants.PieceNotation.Knight,
            Constants.PieceNotation.Bishop,
            Constants.PieceNotation.Rook,
            Constants.PieceNotation.Queen,
            Constants.PieceNotation.King,
        };

        private static readonly char[] SquareDelimiters = new char[]
        {
            Constants.MoveType.Capture,
            Constants.MoveType.Move
        };

        // TODO: Move this to another class?
        public static Square ParseSquare(string input)
        {
            var file = Char.ToLower(input[0]);
            var rank = input[1] - '0';

            return new Square(file, rank);
        }

        // Parses a move, but does not determine validity
        public static bool TryParseMove(string input, BoardState state, ulong piecesOnCurrentSide, out Move result)
        {
            // ✔ Account for castling (O-O, O-O-O)
            // ✔ Account for regular move (a4, Ng8)
            // ✔ Account for long-form move (Nb1 c3, Nb1-c3)
            // ✔ Account for long-form capture (Ne2xa4)
            // ✔ Account for short-form capture (Nxg4, axb4)
            // ✔ Account for disambiguation (Ngg4)
            // ✔ Account for piece promotions (e8=Q)
            //   ✔ Account for promoted piece must be own colour
            //   ✔ Account for if pawn moves to end without specifying promotion
            //   ✔ Account for if non-pawn specifies promotion
            //   ✔ Account for if promotion piece tries to become pawn
            //   ✔ Account for if specify promotion when not at end
            // ✔ Account for state change marks (Na4+, e2#)
            // ✔ Account for annotations (Na4!, e2??, b5!?)
            // ✔ Account for whitespace (Ne2 x a4) - fail in this case
            // ✔ Account for invalid ranks/files - fail in this case


            var trimmedInput = input.AsSpan().Trim();
            result = new Move();

            if (trimmedInput.IsEmpty || trimmedInput.IndexOf(' ') != -1)
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

            // Read leading piece designation
            char pieceDesignation;
            if (PieceDesignations.Contains(moveNotation[0]))
            {
                pieceDesignation = moveNotation[0];
                moveNotation = moveNotation.Slice(1);
            }
            else
            {
                pieceDesignation = Constants.PieceNotation.Pawn;
            }


            int squareIdx = moveNotation.Length - 1;

            // Get end square (must be present)
            result.EndRank = moveNotation[squareIdx--] - '0';
            result.EndFile = Char.ToLower(moveNotation[squareIdx--]);


            // Read promotion (if present)
            var promotion = GetPromotion(moveDescriptors, isWhiteMove);
            var lastRankForColor = isWhiteMove ? Constants.Board.NumberOfRows : 1;
            // TODO: Move promotion validation + tests to Game.Move() ??
            if (promotion != 0)
            {
                if (pieceDesignation != Constants.PieceNotation.Pawn) // promoted non-pawn
                    return false;
                else if ((promotion & SquareContents.Pawn) != 0) // promoted to invalid piece (pawn)
                    return false;

                if (result.EndRank != lastRankForColor) // promoted but not at end
                    return false;

                result.PromotedPiece = promotion;
            }
            else if (pieceDesignation == Constants.PieceNotation.Pawn && result.EndRank == lastRankForColor)
            {
                // Moved pawn to end without promoting
                return false;
            }

            // Ignore delimiter (if present)
            bool isCapture = false;
            if (squareIdx >= 0 && SquareDelimiters.Contains(moveNotation[squareIdx]))
            {
                isCapture = moveNotation[squareIdx] == Constants.MoveType.Capture;
                squareIdx--;
            }

            // Read start squares (if present)
            if (squareIdx >= 0 && Char.IsDigit(moveNotation[squareIdx]))
                result.StartRank = moveNotation[squareIdx--] - '0';

            if (squareIdx >= 0 && Char.IsLetter(moveNotation[squareIdx]))
                result.StartFile = Char.ToLower(moveNotation[squareIdx--]);

            if (result.StartRank != 0 && result.StartFile != 0)
                return IsMoveValid(result);

            if (TryInferStartSquare(state, piecesOnCurrentSide, isWhiteMove, isCapture, pieceDesignation, ref result))
                return IsMoveValid(result);

            return false;
        }

        private static bool IsMoveValid(Move move)
        {
            return move.StartFile >= 'a' && move.StartFile <= 'h'
                && move.EndFile >= 'a' && move.EndFile <= 'h'
                && move.StartRank >= 1 && move.StartRank <= 8
                && move.EndRank >= 1 && move.EndRank <= 8;
        }

        private static bool TryInferStartSquare(BoardState state, ulong piecesOnCurrentSide, bool isWhiteMove, bool isCapture, char pieceDesignation, ref Move result)
        {
            var endBit = BitTranslator.TranslateToBit(result.EndFile, result.EndRank);
            var disambiguityMask = BuildDisambiguityMask(result);

            switch (pieceDesignation)
            {
                case Constants.PieceNotation.King:
                    {
                        var possibleStartBits = MoveGenerator.GetKingMovements(endBit);
                        var existingOfPiece = state.Kings & piecesOnCurrentSide;
                        var actualStartPiece = possibleStartBits & existingOfPiece;

                        if (disambiguityMask != 0)
                            actualStartPiece &= disambiguityMask;

                        if (actualStartPiece == 0)
                            return false;

                        var startSquare = BitTranslator.TranslateToSquare(actualStartPiece);

                        result.StartFile = startSquare.File;
                        result.StartRank = startSquare.Rank;
                        return true;
                    }

                case Constants.PieceNotation.Knight:
                    {
                        var possibleStartBits = MoveGenerator.GetKnightMovements(endBit);
                        var existingOfPiece = state.Knights & piecesOnCurrentSide;
                        var actualStartPiece = possibleStartBits & existingOfPiece;

                        if (disambiguityMask != 0)
                            actualStartPiece &= disambiguityMask;

                        if (actualStartPiece == 0)
                            return false;

                        var startSquare = BitTranslator.TranslateToSquare(actualStartPiece);

                        result.StartFile = startSquare.File;
                        result.StartRank = startSquare.Rank;
                        return true;
                    }

                case Constants.PieceNotation.Queen:
                    {
                        var possibleStartBits = MoveGenerator.GetQueenMovements(endBit, state);
                        var existingOfPiece = state.Queens & piecesOnCurrentSide;
                        var actualStartPiece = possibleStartBits & existingOfPiece;

                        if (disambiguityMask != 0)
                            actualStartPiece &= disambiguityMask;

                        if (actualStartPiece == 0)
                            return false;

                        var startSquare = BitTranslator.TranslateToSquare(actualStartPiece);

                        result.StartFile = startSquare.File;
                        result.StartRank = startSquare.Rank;
                        return true;
                    }

                case Constants.PieceNotation.Rook:
                    {
                        var possibleStartBits = MoveGenerator.GetRookMovements(endBit, state);
                        var existingOfPiece = state.Rooks & piecesOnCurrentSide;
                        var actualStartPiece = possibleStartBits & existingOfPiece;

                        if (disambiguityMask != 0)
                            actualStartPiece &= disambiguityMask;

                        if (actualStartPiece == 0)
                            return false;

                        var startSquare = BitTranslator.TranslateToSquare(actualStartPiece);

                        result.StartFile = startSquare.File;
                        result.StartRank = startSquare.Rank;
                        return true;
                    }

                case Constants.PieceNotation.Bishop:
                    {
                        var possibleStartBits = MoveGenerator.GetBishopMovements(endBit, state);
                        var existingOfPiece = state.Bishops & piecesOnCurrentSide;
                        var actualStartPiece = possibleStartBits & existingOfPiece;

                        if (disambiguityMask != 0)
                            actualStartPiece &= disambiguityMask;

                        if (actualStartPiece == 0)
                            return false;

                        var startSquare = BitTranslator.TranslateToSquare(actualStartPiece);

                        result.StartFile = startSquare.File;
                        result.StartRank = startSquare.Rank;
                        return true;
                    }

                case Constants.PieceNotation.Pawn:
                    {
                        ulong actualStartPiece;
                        if (isWhiteMove)
                        {
                            var possibleMoves = endBit >> 8 | endBit >> 16;
                            var possibleAttacks = (endBit >> 7 & (~MoveGenerator.Rank8)) | (endBit >> 9 & (~MoveGenerator.Rank1));
                            var possibleStart = isCapture ? possibleAttacks : possibleMoves;
                            var pawns = state.Pawns & state.WhitePieces;
                            actualStartPiece = pawns & possibleStart;
                        }
                        else
                        {
                            var possibleMoves = endBit << 8 | endBit << 16;
                            var possibleAttacks = (endBit << 7 & (~MoveGenerator.Rank1)) | (endBit << 9 & (~MoveGenerator.Rank8));
                            var possibleStart = isCapture ? possibleAttacks : possibleMoves;
                            var pawns = state.Pawns & state.BlackPieces;
                            actualStartPiece = pawns & possibleStart;
                        }

                        if (disambiguityMask != 0)
                            actualStartPiece &= disambiguityMask;

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

        private static ulong BuildDisambiguityMask(Move result)
        {
            if (result.StartRank != 0)
            {
                int shift = (result.StartRank - 1) * 8;
                return MoveGenerator.Rank1 << shift;
            }
            else if (result.StartFile != 0)
            {
                int shift = (result.StartFile - 'a');
                return MoveGenerator.FileA << shift;
            }
            else
                return 0;
        }

        private static ReadOnlySpan<char> TrimMoveDescriptors(ReadOnlySpan<char> move)
        {
            for (var i = move.Length - 1; i >= 0; i--)
                if (Char.IsDigit(move[i]) || move[i] == 'O')
                    return move.Slice(0, i + 1);

            return ReadOnlySpan<char>.Empty;
        }

        private static bool TryHandleCastling(ReadOnlySpan<char> moveNotation, bool isWhiteMove, ref Move result)
        {
            var castleKingside = moveNotation.Equals("O-O".AsSpan(), StringComparison.OrdinalIgnoreCase);
            var castleQueenside = moveNotation.Equals("O-O-O".AsSpan(), StringComparison.OrdinalIgnoreCase);

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
                return GetSquareContents(descriptor[1], isWhiteTurn);

            return SquareContents.Empty;
        }

        private static SquareContents GetSquareContents(char piece, bool isWhiteTurn)
        {
            SquareContents side = isWhiteTurn ? SquareContents.White : SquareContents.Black;
            switch (piece)
            {
                case Constants.PieceNotation.King:
                    return SquareContents.King | side;
                case Constants.PieceNotation.Queen:
                    return SquareContents.Queen | side;
                case Constants.PieceNotation.Rook:
                    return SquareContents.Rook | side;
                case Constants.PieceNotation.Bishop:
                    return SquareContents.Bishop | side;
                case Constants.PieceNotation.Knight:
                    return SquareContents.Knight | side;
                case Constants.PieceNotation.Pawn:
                    return SquareContents.Pawn | side;
            }

            return SquareContents.Empty;
        }
    }
}
