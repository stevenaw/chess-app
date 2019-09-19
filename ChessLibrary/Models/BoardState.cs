﻿namespace ChessLibrary.Models
{
    // TODO: Make this a struct?
    internal class BoardState
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
        public ulong WhitePieces { get; set; }
        public ulong BlackPieces { get; set; }

        public ulong Pawns { get; set; }
        public ulong Bishops { get; set; }
        public ulong Knights { get; set; }
        public ulong Rooks { get; set; }
        public ulong Kings { get; set; }
        public ulong Queens { get; set; }

        internal BoardState Copy()
        {
            var newState = new BoardState();

            newState.WhitePieces = this.WhitePieces;
            newState.BlackPieces = this.BlackPieces;

            newState.Pawns = this.Pawns;
            newState.Bishops = this.Bishops;
            newState.Knights = this.Knights;
            newState.Rooks = this.Rooks;
            newState.Kings = this.Kings;
            newState.Queens = this.Queens;

            return newState;
        }

        public ulong AllPieces
        {
            get
            {
                return WhitePieces | BlackPieces;
            }
        }

        public static BoardState Empty
        {
            get
            {
                return new BoardState();
            }
        }

        public static BoardState DefaultPositions
        {
            get
            {
                var v = new BoardState();

                v.WhitePieces = 0b_1111_1111_1111_1111;
                v.Pawns = 0b_1111_1111_0000_0000;
                v.Rooks = 0b_1000_0001;
                v.Knights = 0b_0100_0010;
                v.Bishops = 0b_0010_0100;
                v.Queens = 0b_0000_1000;
                v.Kings = 0b_0001_0000;

                v.BlackPieces = (ulong)0b_1111_1111_1111_1111 << (8 * 6);
                v.Pawns |= v.Pawns << (8 * 5);
                v.Rooks |= v.Rooks << (8 * 7);
                v.Knights |= v.Knights << (8 * 7);
                v.Bishops |= v.Bishops << (8 * 7);
                v.Queens |= v.Queens << (8 * 7);
                v.Kings |= v.Kings << (8 * 7);

                return v;
            }
        }
    }

}
