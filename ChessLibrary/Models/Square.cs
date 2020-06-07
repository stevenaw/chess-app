namespace ChessLibrary.Models
{
    public readonly struct Square
    {
        public readonly char File { get; }
        public readonly int Rank { get; }

        public Square(char file, int rank)
        {
            File = file;
            Rank = rank;
        }
    }
}
