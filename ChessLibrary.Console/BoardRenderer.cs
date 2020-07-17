using ChessLibrary.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace ChessLibrary.ConsoleApp
{
    internal static class BoardRenderer
    {
        private static Dictionary<SquareContents, char> PiecesAsLetters => new Dictionary<SquareContents, char>
        {
            { SquareContents.Empty, ' ' },

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

        public static void PrintBoard(Game game, IEnumerable<Square> highlighted)
        {
            const string rankDivider = "  -----------------";

            using var output = new StreamWriter(Console.OpenStandardOutput());

            Span<bool> highlightSquares = stackalloc bool[64];
            foreach (var sq in highlighted)
                highlightSquares[BitTranslator.GetSquareIndex(sq.File, sq.Rank)] = true;

            output.Write(rankDivider);
            output.Write(Environment.NewLine);

            for (var rank = 8; rank > 0; rank--)
            {
                output.Write((char)(rank + '0'));
                output.Write(" |");

                for (var file = 'a'; file <= 'h'; file++)
                {
                    var contents = game.GetSquareContents(file, rank);
                    var representation = PiecesAsLetters[contents];

                    var currentIndex = BitTranslator.GetSquareIndex(file, rank);

                    if (highlightSquares[currentIndex])
                    {
                        output.Flush();

                        Console.BackgroundColor = ConsoleColor.Yellow;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.Write(representation);

                        Console.ResetColor();
                    }
                    else
                    {
                        output.Write(representation);
                    }

                    output.Write('|');
                }

                output.Write(Environment.NewLine);
                output.Write(rankDivider);
                output.Write(Environment.NewLine);
            }

            output.Write("  ");
            for (var file = 'A'; file <= 'H'; file++)
            {
                output.Write(' ');
                output.Write(file);
            }
            output.Write(Environment.NewLine);
            output.Write(Environment.NewLine);

            output.Flush();
        }

        public static void PrintBoard(Game game)
        {
            PrintBoard(game, Array.Empty<Square>());
        }
    }
}
