using System.Diagnostics;

namespace ChessLibrary.Models
{
    [DebuggerDisplay("{File}{Rank}")]
    public readonly struct Square
    {
        public readonly char File;
        public readonly int Rank;

        public Square(char file, int rank)
        {
            File = file;
            Rank = rank;
        }

        public override bool Equals(object? obj)
        {
            return (obj is Square other) && Equals(other);
        }

        public bool Equals(Square obj)
        {
            return (File == obj.File && Rank == obj.Rank);
        }

        public override int GetHashCode()
        {
            return (File << 16) | (Rank & 16);
        }

        public static bool operator ==(Square left, Square right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Square left, Square right)
        {
            return !(left == right);
        }
    }
}
