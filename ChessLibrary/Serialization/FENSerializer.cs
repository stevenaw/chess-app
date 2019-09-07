using ChessLibrary.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChessLibrary.Serialization
{
    internal class FENSerializer
    {
        private const int MaximumLength = 71;
        private const char FenDelimeter = '/';
        public const string FEN_START = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR";

        /*
         * Generates a string representation of the board position following FEN Diagram rules
         * 
         * These rules involve having a slash to indicate a new row, and rows are processed from high
         * to low (black's home row to white's home row). Each piece is represented by its algebraic
         * notational character, with case indicative of color (white is uppercase, black is lowercase)
         */
        //public string Serialize(Board obj)
        //{
        //    char[] endResult = new char[MaximumLength];
        //    int currIndex = 0;
        //    int numBlanks = 0;
        //    Piece p;

        //    for (int row = Board.NUM_ROWS - 1; row >= 0; row--)
        //    {
        //        for (int col = 0; col < Board.NUM_FILES; col++)
        //        {
        //            p = obj.ActiveGame[row, col];

        //            // If not piece, increment numBlanks
        //            if (p == null)
        //                numBlanks++;
        //            else
        //            {
        //                // Output blanks before piece if necessary
        //                if (numBlanks != 0)
        //                {
        //                    endResult[currIndex++] = ((char)(numBlanks + BoardSquare.YCOORD_OFFSET - 1));
        //                    numBlanks = 0;
        //                }

        //                // Output piece, black as lower case
        //                if (p.Colour == Piece.NOTATION_B)
        //                    endResult[currIndex++] = Char.ToLower(p.Notational);
        //                else
        //                    endResult[currIndex++] = Char.ToUpper(p.Notational);
        //            }
        //        }

        //        // If blanks to output at end of line, output
        //        if (numBlanks != 0)
        //        {
        //            endResult[currIndex++] = ((char)(numBlanks + BoardSquare.YCOORD_OFFSET - 1));
        //            numBlanks = 0;
        //        }

        //        // Add line delimiter if not last row
        //        if (row != 0)
        //            endResult[currIndex++] = FenDelimeter;
        //    }

        //    // Return as a string
        //    return (new String(endResult, 0, currIndex));
        //}

        public BoardState Deserialize(string input)
        {
            BoardState board = new BoardState();
            int currRow = Constants.Board.NumberOfRows - 1;
            int currFile = 0;
            char lastSymbol = '-';  // Initialize to unexpected symbol

            if (String.IsNullOrEmpty(input))
                throw new ArgumentNullException(nameof(input));

            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == FenDelimeter)
                {
                    if (currFile != Constants.Board.NumberOfFiles)
                        throw new InvalidOperationException("Unanticipated new row, columns to fill!");
                    else if (--currRow < 0)
                        throw new InvalidOperationException("Too many rows!");

                    currFile = 0;
                }
                else if (Char.IsDigit(input[i]))
                {
                    int gap = input[i] - '0';

                    if (Char.IsDigit(lastSymbol))
                        throw new InvalidOperationException("Can not specify two consecutive blank spots!");
                    else if (currFile + gap > Constants.Board.NumberOfFiles)
                        throw new InvalidOperationException("Blank spot exceeds column count!");
                    else if (gap == 0)
                        throw new InvalidOperationException("Can not have a 0 in a FEN diagram!");

                    // TODO: currFile += gap
                    while (gap-- > 0)
                        currFile++;
                }
                else if (Char.IsLetter(input[i]))
                {
                    if (currFile >= Constants.Board.NumberOfFiles)
                        throw new InvalidOperationException("Too many pieces specified in row!");

                    char currPiece = Char.ToUpper(input[i]);

                    SquareContents colour = Char.IsUpper(input[i]) ? SquareContents.White : SquareContents.Black;
                    SquareContents piece;

                    switch (currPiece)
                    {
                        case Constants.PieceNotation.Pawn:
                            piece = SquareContents.Pawn;
                            break;
                        case Constants.PieceNotation.Knight:
                            piece = SquareContents.Knight;
                            break;
                        case Constants.PieceNotation.Bishop:
                            piece = SquareContents.Bishop;
                            break;
                        case Constants.PieceNotation.Rook:
                            piece = SquareContents.Rook;
                            break;
                        case Constants.PieceNotation.Queen:
                            piece = SquareContents.Queen;
                            break;
                        case Constants.PieceNotation.King:
                            piece = SquareContents.King;
                            break;
                        default:
                            throw new InvalidOperationException("Unsupported piece notation: " + input[i]);
                    }

                    var squareIdx = currRow * Constants.Board.NumberOfFiles + currFile;
                    BoardStateManipulator.SetPiece(board, 1UL << squareIdx, colour | piece);
                    currFile++;
                }
                else
                    throw new InvalidOperationException("Unsupported symbol: " + input[i]);

                lastSymbol = input[i];
            }

            if (currRow != 0)
                throw new InvalidOperationException("Not enough rows specified!");
            else if (currFile != 8)
                throw new InvalidOperationException("Not enough columns specified!");
            else
                return board;
        }
    }
}
