namespace ChessLibrary.Models
{
    public enum AttackState
    {
        None = 0,
        Check,      // +
        Checkmate,  // #
        Stalemate,
        DrawByRepetition,
        DrawByInactivity,
    }
}
