using ChessLibrary.Models;
using System.Runtime.CompilerServices;

namespace ChessLibrary
{
    public static class BitTranslator
    {
        internal static bool IsValidSquare(char file, int rank)
        {
            if (rank < 1 || rank > 8)
                return false;

            int fileNumber = Char.ToLower(file) - 'a';

            if (fileNumber < 0 || fileNumber > 7)
                return false;
            return true;
        }

        internal static ulong TranslateToBit(char file, int rank)
        {
            int idx = GetSquareIndex(file, rank);
            ulong result = 1UL << idx;
            return result;
        }

        public static int GetSquareIndex(char file, int rank)
        {
            int fileNumber = Char.ToLower(file) - 'a';
            int idx = ((rank - 1) * 8) + fileNumber;
            return idx;
        }

        private static readonly Dictionary<ulong, Square> BitSquares = GenerateBitSquares();

        private static Dictionary<ulong, Square> GenerateBitSquares()
        {
            var map = new Dictionary<ulong, Square>();

            for (var i = 0; i < 64; i++)
            {
                int rank = (i / 8) + 1;
                char file = (char)('a' + (i % 8));

                map[1UL << i] = new Square(file, rank);
            }

            return map;
        }

        internal static List<Square> TranslateToSquares(ulong square)
        {
            var result = new List<Square>();

            for (var i = 0; square != 0; i++)
            {
                if ((square & 1) != 0)
                {
                    int squareRank = (i / 8) + 1;
                    char squareFile = (char)('a' + (i % 8));

                    result.Add(new Square(squareFile, squareRank));
                }

                square >>= 1;
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static Square TranslateToSquare(ulong square)
        {
            return BitSquares[square];
        }
    }
}
