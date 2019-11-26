using System.Runtime.CompilerServices;

namespace ChessLibrary.Models
{
    internal readonly struct BoardState
    {
        /* 64bit long = 64 square board
         * Bit placement = squares
         * 
         *    |---------------------------------------------------------------|
         *  8 | 1<<56 | 1<<57 | 1<<58 | 1<<59 | 1<<60 | 1<<61 | 1<<62 | 1<<63 |
         *    |---------------------------------------------------------------|
         *  7 | 1<<48 | 1<<49 | 1<<50 | 1<<51 | 1<<52 | 1<<53 | 1<<54 | 1<<55 |
         *    |---------------------------------------------------------------|
         *  6 | 1<<40 | 1<<41 | 1<<42 | 1<<43 | 1<<44 | 1<<45 | 1<<46 | 1<<47 |
         *    |---------------------------------------------------------------|
         *  5 | 1<<32 | 1<<33 | 1<<34 | 1<<35 | 1<<36 | 1<<37 | 1<<38 | 1<<39 |
         *    |---------------------------------------------------------------|
         *  4 | 1<<24 | 1<<25 | 1<<26 | 1<<27 | 1<<28 | 1<<29 | 1<<30 | 1<<31 |
         *    |---------------------------------------------------------------|
         *  3 | 1<<16 | 1<<17 | 1<<18 | 1<<19 | 1<<20 | 1<<21 | 1<<22 | 1<<23 |
         *    |---------------------------------------------------------------|
         *  2 | 1<< 8 | 1<< 9 | 1<<10 | 1<<11 | 1<<12 | 1<<13 | 1<<14 | 1<<15 |
         *    |---------------------------------------------------------------|
         *  1 | 1<< 0 | 1<< 1 | 1<< 2 | 1<< 3 | 1<< 4 | 1<< 5 | 1<< 6 | 1<< 7 |
         *    |---------------------------------------------------------------|
         *        A       B       C       D       E       F       G       H
         */
        public ulong WhitePieces { get; }
        public ulong BlackPieces { get; }

        public ulong Pawns { get; }
        public ulong Bishops { get; }
        public ulong Knights { get; }
        public ulong Rooks { get; }
        public ulong Kings { get; }
        public ulong Queens { get; }

        public ulong AllPieces => WhitePieces | BlackPieces;

        public BoardState(
            ulong white, ulong black, ulong pawns, ulong knights,
            ulong bishops, ulong rooks, ulong queens, ulong kings
        )
        {
            this.WhitePieces = white;
            this.BlackPieces = black;
            this.Pawns = pawns;
            this.Knights = knights;
            this.Bishops = bishops;
            this.Rooks = rooks;
            this.Queens = queens;
            this.Kings = kings;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BoardState SetPiece(ulong bitSquare, SquareContents contents)
        {
            return BoardStateMutator.SetPiece(this, bitSquare, contents);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal BoardState SetPiece(Square square, SquareContents contents)
        {
            return BoardStateMutator.SetPiece(this, BitTranslator.TranslateToBit(square.File, square.Rank), contents);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BoardState MovePiece(ulong from, ulong to)
        {
            return BoardStateMutator.MovePiece(this, from, to);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BoardState Copy()
        {
            return BoardStateMutator.Copy(this);
        }

        public static BoardState Empty { get; } = new BoardState();

        public static BoardState DefaultPositions { get; } = GetDefaultPositions();

        private static BoardState GetDefaultPositions()
        {
            ulong white     = 0b_1111_1111_1111_1111;
            ulong black     = (ulong)0b_1111_1111_1111_1111 << (8 * 6);
            ulong pawns     = 0xFF00 | ((ulong)0xFF00 << (8 * 5));
            ulong rooks     = 0b_1000_0001 | ((ulong)0b_1000_0001 << (8 * 7));
            ulong knights   = 0b_0100_0010 | ((ulong)0b_0100_0010 << (8 * 7));
            ulong bishops   = 0b_0010_0100 | ((ulong)0b_0010_0100 << (8 * 7));
            ulong queens    = 0b_0000_1000 | ((ulong)0b_0000_1000 << (8 * 7));
            ulong kings     = 0b_0001_0000 | ((ulong)0b_0001_0000 << (8 * 7));

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

        public override bool Equals(object obj)
        {
            if (obj is BoardState bs)
                return Equals(this, bs);

            return false;
        }

        public bool Equals(BoardState other)
        {
            return Equals(this, other);
        }

        private static bool Equals(BoardState a, BoardState b)
        {
            return a.Bishops == b.Bishops
                && a.BlackPieces == b.BlackPieces
                && a.Kings == b.Kings
                && a.Knights == b.Knights
                && a.Pawns == b.Pawns
                && a.Queens == b.Queens
                && a.Rooks == b.Rooks
                && a.WhitePieces == b.WhitePieces;
        }
    }

}
