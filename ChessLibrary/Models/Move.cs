namespace ChessLibrary.Models
{
    // TODO: Immutable
    public struct Move
    {
        public char StartFile { get; set; }
        public int StartRank { get; set; }
        public char EndFile { get; set; }
        public int EndRank { get; set; }

        public MoveAnnotation Annotation { get; set; }
        public AttackState AttackState { get; set; }
        public SquareContents PromotedPiece { get; set; }

        private static Move _empty = new Move();
        public static Move Empty => _empty;

        public static bool Equals(Move a, Move b)
        {
            return a.Annotation == b.Annotation
                && a.AttackState == b.AttackState
                && a.EndFile == b.EndFile
                && a.EndRank == b.EndRank
                && a.PromotedPiece == b.PromotedPiece
                && a.StartFile == b.StartFile
                && a.StartRank == b.StartRank;
        }
    }
}
