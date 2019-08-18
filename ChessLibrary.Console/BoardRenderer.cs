using ChessLibrary.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChessLibrary.ConsoleApp
{
    public static class BoardRenderer
    {
        private static Dictionary<SquareContents, char> PiecesAsLetters => new Dictionary<SquareContents, char>
        {
            { SquareContents.White | SquareContents.King, 'K' },
            { SquareContents.White | SquareContents.Queen, 'Q' },
            { SquareContents.White | SquareContents.Rook, 'R' },
            { SquareContents.White | SquareContents.Bishop, 'B' },
            { SquareContents.White | SquareContents.Knight, 'N' },
            { SquareContents.White | SquareContents.Pawn, 'P' },

            { SquareContents.Black | SquareContents.King, 'k' },
            { SquareContents.Black | SquareContents.Queen, 'q' },
            { SquareContents.Black | SquareContents.Rook, 'r' },
            { SquareContents.Black | SquareContents.Bishop, 'b' },
            { SquareContents.Black | SquareContents.Knight, 'n' },
            { SquareContents.Black | SquareContents.Pawn, 'p' },
        };
        private static Dictionary<SquareContents, char> PiecesAsGlyphs => new Dictionary<SquareContents, char>
        {
            { SquareContents.White | SquareContents.King, '♔' },
            { SquareContents.White | SquareContents.Queen, '♕' },
            { SquareContents.White | SquareContents.Rook, '♖' },
            { SquareContents.White | SquareContents.Bishop, '♗' },
            { SquareContents.White | SquareContents.Knight, '♘' },
            { SquareContents.White | SquareContents.Pawn, '♙' },

            { SquareContents.Black | SquareContents.King, '♚' },
            { SquareContents.Black | SquareContents.Queen, '♛' },
            { SquareContents.Black | SquareContents.Rook, '♜' },
            { SquareContents.Black | SquareContents.Bishop, '♝' },
            { SquareContents.Black | SquareContents.Knight, '♞' },
            { SquareContents.Black | SquareContents.Pawn, '♟' },
        };

        public static void PrintBoard(Game game)
        {
            const string rankDivider = "  -----------------";

            // This could instead just be a char[] or Span<char>
            // At 2 EOL markers (\r\n) it's 380 in length
            var output = new StringBuilder(
                // row = 8 squares + row number. Each has a delimiter (so *2)
                ((8 + 1) * 2 + Environment.NewLine.Length + 1)
                // rows to output = 8 squares + delimiter for each
                * (8 + 1) * 2
                 + Environment.NewLine.Length
            );

            output.Append(rankDivider);
            output.Append(Environment.NewLine);

            for (var rank = 8; rank > 0; rank--)
            {
                output.Append((char)(rank + '0'));
                output.Append(" |");

                for (var file = 'a'; file <= 'h'; file++)
                {
                    var contents = game.GetSquareContents(file, rank);
                    var representation = GetPieceRepresentation(contents);

                    output.Append(representation);
                    output.Append('|');
                }

                output.Append(Environment.NewLine);
                output.Append(rankDivider);
                output.Append(Environment.NewLine);
            }

            output.Append("  ");
            for (var file = 'A'; file <= 'H'; file++)
            {
                output.Append(' ');
                output.Append(file);
            }
            output.Append(Environment.NewLine);
            output.Append(Environment.NewLine);

            Console.Write(output.ToString());
        }

        private static char GetPieceRepresentation(SquareContents contents)
        {
            if (contents == 0)
                return ' ';
            return PiecesAsLetters[contents];
        }
    }
}
