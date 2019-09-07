using ChessLibrary.Models;
using System;

namespace ChessLibrary.Serialization
{
    internal class FenSerializer
    {
        private const int MaximumLength = 71;
        private const char FenDelimeter = '/';
        public string Serialize(BoardState board)
        {
            Span<char> endResult = stackalloc char[MaximumLength];
            var currIndex = 0;
            var numBlanks = 0;

            for (var rank = Constants.Board.NumberOfRows - 1; rank >= 0; rank--)
            {
                for (int file = 0; file < Constants.Board.NumberOfFiles; file++)
                {
                    var squareIdx = rank * Constants.Board.NumberOfRows + file;
                    var squareMask = 1UL << squareIdx;

                    // If not piece, increment numBlanks
                    if ((board.AllPieces & squareMask) == 0)
                        numBlanks++;
                    else
                    {
                        // Output blanks before piece if necessary
                        if (numBlanks != 0)
                        {
                            endResult[currIndex++] = (char)(numBlanks + '0');
                            numBlanks = 0;
                        }

                        endResult[currIndex++] = ToNotation(board, squareMask);
                    }
                }

                // If blanks to output at end of line, output
                if (numBlanks != 0)
                {
                    endResult[currIndex++] = (char)(numBlanks + '0');
                    numBlanks = 0;
                }

                // Add line delimiter if not last row
                if (rank != 0)
                    endResult[currIndex++] = FenDelimeter;
            }

            // Return as a string
            return endResult.Slice(0, currIndex).ToString();
        }

        public BoardState Deserialize(string input)
        {
            var board = new BoardState();
            var currRank = Constants.Board.NumberOfRows - 1;
            var currFile = 0;
            var lastSymbol = '-';  // Initialize to unexpected symbol

            if (String.IsNullOrEmpty(input))
                throw new ArgumentNullException(nameof(input));
            if (input.Length > MaximumLength)
                throw new ArgumentException("Input is too long", nameof(input));

            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == FenDelimeter)
                {
                    if (currFile != Constants.Board.NumberOfFiles)
                        throw new InvalidOperationException("Unanticipated new row, columns to fill!");
                    else if (--currRank < 0)
                        throw new InvalidOperationException("Too many rows!");

                    currFile = 0;
                }
                else if (Char.IsDigit(input[i]))
                {
                    var gap = input[i] - '0';

                    if (Char.IsDigit(lastSymbol))
                        throw new InvalidOperationException("Can not specify two consecutive blank spots!");
                    else if (currFile + gap > Constants.Board.NumberOfFiles)
                        throw new InvalidOperationException("Blank spot exceeds column count!");
                    else if (gap == 0)
                        throw new InvalidOperationException("Can not have a 0 in a FEN diagram!");

                    currFile += gap;
                }
                else if (Char.IsLetter(input[i]))
                {
                    if (currFile >= Constants.Board.NumberOfFiles)
                        throw new InvalidOperationException("Too many pieces specified in row!");

                    var piece = FromNotation(input[i]);
                    var squareIdx = currRank * Constants.Board.NumberOfFiles + currFile;

                    BoardStateManipulator.SetPiece(board, 1UL << squareIdx, piece);
                    currFile++;
                }
                else
                    throw new InvalidOperationException("Unsupported symbol: " + input[i]);

                lastSymbol = input[i];
            }

            if (currRank != 0)
                throw new InvalidOperationException("Not enough rows specified!");
            else if (currFile != 8)
                throw new InvalidOperationException("Not enough columns specified!");
            else
                return board;
        }

        private static SquareContents FromNotation(char symbol)
        {
            var colour = Char.IsUpper(symbol) ? SquareContents.White : SquareContents.Black;

            switch (Char.ToUpper(symbol))
            {
                case Constants.PieceNotation.Pawn:
                    return colour | SquareContents.Pawn;
                case Constants.PieceNotation.Knight:
                    return colour | SquareContents.Knight;
                case Constants.PieceNotation.Bishop:
                    return colour | SquareContents.Bishop;
                case Constants.PieceNotation.Rook:
                    return colour | SquareContents.Rook;
                case Constants.PieceNotation.Queen:
                    return colour | SquareContents.Queen;
                case Constants.PieceNotation.King:
                    return colour | SquareContents.King;
                default:
                    throw new InvalidOperationException("Unsupported piece notation: " + symbol);
            }
        }

        private static char ToNotation(BoardState board, ulong squareMask)
        {
            var symbol = '\0';
            if ((board.Kings & squareMask) != 0)
                symbol = Constants.PieceNotation.King;
            else if ((board.Queens & squareMask) != 0)
                symbol = Constants.PieceNotation.Queen;
            else if ((board.Rooks & squareMask) != 0)
                symbol = Constants.PieceNotation.Rook;
            else if ((board.Bishops & squareMask) != 0)
                symbol = Constants.PieceNotation.Bishop;
            else if ((board.Knights & squareMask) != 0)
                symbol = Constants.PieceNotation.Knight;
            else if ((board.Pawns & squareMask) != 0)
                symbol = Constants.PieceNotation.Pawn;

            if ((board.BlackPieces & squareMask) != 0)
                symbol = Char.ToLower(symbol);
            return symbol;
        }
    }
}
