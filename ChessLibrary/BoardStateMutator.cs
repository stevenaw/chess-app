using ChessLibrary.Models;

namespace ChessLibrary
{
    internal static class BoardStateMutator
    {
        internal static BoardState Copy(BoardState state)
        {
            return new BoardState(
                state.WhitePieces,
                state.BlackPieces,
                state.Pawns,
                state.Knights,
                state.Bishops,
                state.Rooks,
                state.Queens,
                state.Kings
            );
        }

        internal static BoardState SetPiece(BoardState state, ulong bitSquare, SquareContents contents)
        {
            var bitContents = (ulong)contents;

            ulong white = state.WhitePieces;
            ulong black = state.BlackPieces;
            ulong pawns = state.Pawns;
            ulong rooks = state.Rooks;
            ulong knights = state.Knights;
            ulong bishops = state.Bishops;
            ulong queens = state.Queens;
            ulong kings = state.Kings;

            if ((bitContents & (int)SquareContents.White) != 0)
                white |= bitSquare;
            else if ((bitContents & (int)SquareContents.Black) != 0)
                black |= bitSquare;


            if ((bitContents & (int)SquareContents.Pawn) != 0)
                pawns |= bitSquare;
            else if ((bitContents & (int)SquareContents.Knight) != 0)
                knights |= bitSquare;
            else if ((bitContents & (int)SquareContents.Bishop) != 0)
                bishops |= bitSquare;
            else if ((bitContents & (int)SquareContents.Rook) != 0)
                rooks |= bitSquare;
            else if ((bitContents & (int)SquareContents.Queen) != 0)
                queens |= bitSquare;
            else if ((bitContents & (int)SquareContents.King) != 0)
                kings |= bitSquare;

            return new BoardState(
                white,
                black,
                pawns,
                knights,
                bishops,
                rooks,
                queens,
                kings
            );
        }

        internal static BoardState MovePiece(BoardState state, ulong from, ulong to)
        {
            state = ClearPiece(state, to);

            ulong white = state.WhitePieces;
            ulong black = state.BlackPieces;
            ulong pawns = state.Pawns;
            ulong rooks = state.Rooks;
            ulong knights = state.Knights;
            ulong bishops = state.Bishops;
            ulong queens = state.Queens;
            ulong kings = state.Kings;

            if ((from & pawns) != 0)
                pawns = MovePiece(pawns, from, to);
            else if ((from & knights) != 0)
                knights = MovePiece(knights, from, to);
            else if ((from & bishops) != 0)
                bishops = MovePiece(bishops, from, to);
            else if ((from & rooks) != 0)
                rooks = MovePiece(rooks, from, to);
            else if ((from & queens) != 0)
                queens = MovePiece(queens, from, to);
            else if ((from & state.Kings) != 0)
                kings = MovePiece(kings, from, to);

            if ((from & white) != 0)
            {
                white = MovePiece(white, from, to);
                black &= ~white;
            }
            else
            {
                black = MovePiece(black, from, to);
                white &= ~black;
            }

            return new BoardState(
                white,
                black,
                pawns,
                knights,
                bishops,
                rooks,
                queens,
                kings
            );
        }

        internal static BoardState ClearPiece(BoardState state, ulong bitSquare)
        {
            return new BoardState(
                state.WhitePieces & ~bitSquare,
                state.BlackPieces & ~bitSquare,
                state.Pawns & ~bitSquare,
                state.Knights & ~bitSquare,
                state.Bishops & ~bitSquare,
                state.Rooks & ~bitSquare,
                state.Queens & ~bitSquare,
                state.Kings & ~bitSquare
            );
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
