namespace ChessLibrary.Models
{
    public readonly struct AnnotatedMove
    {
        public readonly Move Move;
        public readonly MoveAnnotation Annotation;
        public readonly AttackState AttackState;

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

        public override bool Equals(object? obj)
        {
            return (obj is AnnotatedMove mv) && Equals(this, mv);
        }

        public override int GetHashCode()
        {
            return Move.GetHashCode() ^ ((int)Annotation) ^ ((int)AttackState);
        }

        public static bool operator ==(AnnotatedMove left, AnnotatedMove right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(AnnotatedMove left, AnnotatedMove right)
        {
            return !Equals(left, right);
        }
    }
}
