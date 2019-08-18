using ChessLibrary.Models;

namespace ChessLibrary
{
    internal static class BoardStateManipulator
    {
        public static void SetPiece(BoardState state, Square square, SquareContents contents)
        {
            var bitSquare = BitTranslator.TranslateToBit(square.File, square.Rank);
            var bitContents = (ulong)contents;

            if ((bitContents & (int)SquareContents.White) != 0)
                state.WhitePieces |= bitSquare;
            else if ((bitContents & (int)SquareContents.Black) != 0)
                state.BlackPieces |= bitSquare;


            if ((bitContents & (int)SquareContents.Pawn) != 0)
                state.Pawns |= bitSquare;
            else if ((bitContents & (int)SquareContents.Knight) != 0)
                state.Knights |= bitSquare;
            else if ((bitContents & (int)SquareContents.Bishop) != 0)
                state.Bishops |= bitSquare;
            else if ((bitContents & (int)SquareContents.Rook) != 0)
                state.Rooks |= bitSquare;
            else if ((bitContents & (int)SquareContents.Queen) != 0)
                state.Queens |= bitSquare;
            else if ((bitContents & (int)SquareContents.King) != 0)
                state.Kings |= bitSquare;
        }

        public static void MovePiece(BoardState state, ulong from, ulong to)
        {
            if ((from & state.Pawns) != 0)
                state.Pawns = MovePiece(state.Pawns, from, to);
            else if ((from & state.Knights) != 0)
                state.Knights = MovePiece(state.Knights, from, to);
            else if ((from & state.Bishops) != 0)
                state.Bishops = MovePiece(state.Bishops, from, to);
            else if ((from & state.Rooks) != 0)
                state.Rooks = MovePiece(state.Rooks, from, to);
            else if ((from & state.Queens) != 0)
                state.Queens = MovePiece(state.Queens, from, to);
            else if ((from & state.Kings) != 0)
                state.Kings = MovePiece(state.Kings, from, to);

            if ((from & state.WhitePieces) != 0)
            {
                state.WhitePieces = MovePiece(state.WhitePieces, from, to);
                state.BlackPieces &= ~state.WhitePieces;
            }
            else
            {
                state.BlackPieces = MovePiece(state.BlackPieces, from, to);
                state.WhitePieces &= ~state.BlackPieces;
            }
        }

        private static ulong MovePiece(ulong field, ulong from, ulong to)
        {
            ulong result = field;

            result |= to;
            result &= ~from;

            return result;
        }
    }
}
