using ChessLibrary.Models;

namespace ChessLibrary
{
    public static class MoveDescriptionHelper
    {
        public static string GetAttackString(AttackState value)
        {
            return value switch
            {
                AttackState.Check => "+",
                AttackState.Checkmate => "#",
                AttackState.None => string.Empty,
                _ => string.Empty,
            };
        }

        public static string GetAnnotationString(MoveAnnotation value)
        {
            return value switch
            {
                MoveAnnotation.Excellent => "!",
                MoveAnnotation.Brilliancy => "!!",
                MoveAnnotation.Mistake => "?",
                MoveAnnotation.Blunder => "??",
                MoveAnnotation.Interesting => "!?",
                MoveAnnotation.Dubious => "?!",
                MoveAnnotation.Normal => string.Empty,
                _ => string.Empty,
            };
        }
    }
}
