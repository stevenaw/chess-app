namespace ChessLibrary
{
    internal static class Constants
    {
        internal static class PieceNotation
        {
            public const char Pawn = 'P';
            public const char Knight = 'N';
            public const char Bishop = 'B';
            public const char Rook = 'R';
            public const char Queen = 'Q';
            public const char King = 'K';
        }

        internal static class MoveType
        {
            public const char Move = '-';
            public const char Capture = 'x';
        }

        internal static class Board
        {
            public const int NumberOfRows = 8;
            public const int NumberOfFiles = 8;
            public const int NumberOfSquares = NumberOfRows * NumberOfFiles;
        }

        internal static class MoveLimits
        {
            public const int RepetitionLimit = 3;
            public const int InactivityLimit = 25 * 2;
        }
    }
}
