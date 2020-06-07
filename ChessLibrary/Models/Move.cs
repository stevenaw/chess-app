namespace ChessLibrary.Models
{
    public readonly struct Move
    {
        public readonly char StartFile { get; }
        public readonly int StartRank { get; }
        public readonly char EndFile { get; }
        public readonly int EndRank { get; }
        public readonly SquareContents PromotedPiece { get; }

        public Move(char startFile, int startRank, char endFile, int endRank) : this(startFile, startRank, endFile, endRank, SquareContents.Empty) { }
        public Move(char startFile, int startRank, char endFile, int endRank, SquareContents promotedPiece)
        {
            StartFile = startFile;
            StartRank = startRank;
            EndFile = endFile;
            EndRank = endRank;
            PromotedPiece = promotedPiece;
        }

        public static Move Empty { get; } = new Move((char)0, 0, (char)0, 0, SquareContents.Empty);

        public static bool Equals(Move a, Move b)
        {
            return a.EndFile == b.EndFile
                && a.EndRank == b.EndRank
                && a.PromotedPiece == b.PromotedPiece
                && a.StartFile == b.StartFile
                && a.StartRank == b.StartRank;
        }

        public override bool Equals(object obj)
        {
            return (obj is Move mv) && Equals(this, mv);
        }

        public override int GetHashCode()
        {
            return (EndFile << 24)
                | ((StartFile & 0xFF) << 16)
                | ((EndRank & 0xF) << 12)
                | ((StartRank & 0xF) << 8)
                | (byte)PromotedPiece;
        }

        public static bool operator ==(Move left, Move right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Move left, Move right)
        {
            return !(left == right);
        }
    }
}
