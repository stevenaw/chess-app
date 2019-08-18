using ChessLibrary.Models;

namespace ChessLibrary
{
    public class MoveDescriptionHelper
    {
        public static string GetAttackString(AttackState value)
        {
            switch (value)
            {
                case AttackState.Check:
                    return "+";
                case AttackState.Checkmate:
                    return "#";
                case AttackState.Normal:
                    return string.Empty;
                default:
                    return string.Empty;
            }
        }

        public static string GetAnnotationString(MoveAnnotation value)
        {
            switch (value)
            {
                case MoveAnnotation.Excellent:
                    return "!";
                case MoveAnnotation.Brilliancy:
                    return "!!";
                case MoveAnnotation.Mistake:
                    return "?";
                case MoveAnnotation.Blunder:
                    return "??";
                case MoveAnnotation.Interesting:
                    return "!?";
                case MoveAnnotation.Dubious:
                    return "?!";
                case MoveAnnotation.Normal:
                    return string.Empty;
                default:
                    return string.Empty;
            }
        }
    }
}
