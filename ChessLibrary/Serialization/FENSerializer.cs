using ChessLibrary.Models;

namespace ChessLibrary.Serialization
{
    internal readonly struct FenSerializer
    {
        private const int MaximumLength = 71;
        private const char FenDelimeter = '/';

        public const string DefaultValue = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR";

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
            return endResult[..currIndex].ToString();
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

                    board = board.SetPiece(1UL << squareIdx, piece);
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

        internal static SquareContents FromNotation(char symbol)
        {
            var colour = Char.IsUpper(symbol) ? SquareContents.White : SquareContents.Black;

            return Char.ToUpper(symbol) switch
            {
                Constants.PieceNotation.Pawn => colour | SquareContents.Pawn,
                Constants.PieceNotation.Knight => colour | SquareContents.Knight,
                Constants.PieceNotation.Bishop => colour | SquareContents.Bishop,
                Constants.PieceNotation.Rook => colour | SquareContents.Rook,
                Constants.PieceNotation.Queen => colour | SquareContents.Queen,
                Constants.PieceNotation.King => colour | SquareContents.King,
                _ => throw new InvalidOperationException("Unsupported piece notation: " + symbol),
            };
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
