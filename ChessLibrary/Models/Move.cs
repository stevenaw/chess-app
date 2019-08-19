namespace ChessLibrary.Models
{
    public struct Move
    {
        public char StartFile { get; set; }
        public int StartRank { get; set; }
        public char EndFile { get; set; }
        public int EndRank { get; set; }

        public MoveAnnotation Annotation { get; set; }
        public AttackState AttackState { get; set; }
        public SquareContents PromotedPiece { get; set; }
    }
}
