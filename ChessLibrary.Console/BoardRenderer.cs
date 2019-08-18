using ChessLibrary.Models;
using System;
using System.Collections.Generic;

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

        public static void PrintBoard(Game game, List<Square> highlighted)
        {
            const string rankDivider = "  -----------------";
            var highlightSquares = new bool[64];

            foreach (var sq in highlighted)
                highlightSquares[BitTranslator.GetSquareIndex(sq.File, sq.Rank)] = true;

            Console.Write(rankDivider);
            Console.Write(Environment.NewLine);

            for (var rank = 8; rank > 0; rank--)
            {
                Console.Write((char)(rank + '0'));
                Console.Write(" |");

                for (var file = 'a'; file <= 'h'; file++)
                {
                    var contents = game.GetSquareContents(file, rank);
                    var representation = GetPieceRepresentation(contents);

                    var currentIndex = BitTranslator.GetSquareIndex(file, rank);

                    if (highlightSquares[currentIndex])
                    {
                        var currentBrush = ConsolePaintBrush.Current;
                        ConsolePaintBrush.Current = ConsolePaintBrush.Highlight;
                        Console.Write(representation);
                        ConsolePaintBrush.Current = currentBrush;
                    }
                    else
                    {
                        Console.Write(representation);
                    }

                    Console.Write('|');
                }

                Console.Write(Environment.NewLine);
                Console.Write(rankDivider);
                Console.Write(Environment.NewLine);
            }

            Console.Write("  ");
            for (var file = 'A'; file <= 'H'; file++)
            {
                Console.Write(' ');
                Console.Write(file);
            }
            Console.Write(Environment.NewLine);
            Console.Write(Environment.NewLine);
        }

        public static void PrintBoard(Game game)
        {
            PrintBoard(game, new List<Square>(0));
        }

        private static char GetPieceRepresentation(SquareContents contents)
        {
            if (contents == 0)
                return ' ';
            return PiecesAsLetters[contents];
        }
    }
}
