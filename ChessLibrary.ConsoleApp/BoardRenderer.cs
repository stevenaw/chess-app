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

        // TODO: Include option to update board in-place (only rows of start, end of move)
        public static void PrintBoard(Game game, IEnumerable<Square> highlighted)
        {
            const string rankDivider = "  -----------------";

            Span<bool> highlightSquares = stackalloc bool[64];
            foreach (var sq in highlighted)
                highlightSquares[BitTranslator.GetSquareIndex(sq.File, sq.Rank)] = true;

            Console.WriteLine(rankDivider);

            for (var rank = 8; rank > 0; rank--)
            {
                Console.Write((char)(rank + '0'));
                Console.Write(" |");

                for (var file = 'a'; file <= 'h'; file++)
                {
                    var contents = game.GetSquareContents(file, rank);
                    var representation = PiecesAsLetters[contents];

                    var currentIndex = BitTranslator.GetSquareIndex(file, rank);

                    if (highlightSquares[currentIndex])
                    {
                        Console.BackgroundColor = ConsoleColor.Yellow;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.Write(representation);

                        Console.ResetColor();
                    }
                    else
                    {
                        Console.Write(representation);
                    }

                    Console.Write('|');
                }

                Console.WriteLine();
                Console.WriteLine(rankDivider);
            }

            Console.Write("  ");
            for (var file = 'A'; file <= 'H'; file++)
            {
                Console.Write(' ');
                Console.Write(file);
            }

            Console.WriteLine();
            Console.WriteLine();
        }

        public static void PrintBoard(Game game)
        {
            PrintBoard(game, Array.Empty<Square>());
        }
    }
}
