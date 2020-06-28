namespace ChessLibrary.Models
{
    public readonly struct AnnotatedMove
    {
        public readonly Move Move { get; }
        public readonly MoveAnnotation Annotation { get; }
        public readonly AttackState AttackState { get; }

        public AnnotatedMove(Move move, MoveAnnotation annotation, AttackState attackState)
        {
            Move = move;
            Annotation = annotation;
            AttackState = attackState;
        }

        public static AnnotatedMove Empty { get; } = new AnnotatedMove(Move.Empty, MoveAnnotation.Normal, AttackState.None);

        public static bool Equals(AnnotatedMove a, AnnotatedMove b)
        {
            return a.Annotation == b.Annotation
                && a.AttackState == b.AttackState
                && a == b;
        }

        public override bool Equals(object obj)
        {
            return (obj is AnnotatedMove mv) && Equals(this, mv);
        }

        public override int GetHashCode()
        {
            return Move.GetHashCode() ^ Annotation.GetHashCode() ^ AttackState.GetHashCode();
        }

        public static bool operator ==(AnnotatedMove left, AnnotatedMove right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(AnnotatedMove left, AnnotatedMove right)
        {
            return !(left == right);
        }
    }
}
