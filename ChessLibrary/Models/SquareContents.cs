using System;

namespace ChessLibrary.Models
{
    [Flags]
    public enum SquareContents
    {
        Pawn = 1,
        Knight = 2,
        Bishop = 4,
        Rook = 8,
        Queen = 16,
        King = 32,

        White = 64,
        Black = 128,

        Colours = White | Black,
        Pieces = Pawn | Knight | Bishop | Rook | Queen | King
    }
}
