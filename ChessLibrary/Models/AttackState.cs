namespace ChessLibrary.Models
{
    public enum AttackState : byte
    {
        None = 0,
        Check,      // +
        Checkmate,  // #
        Stalemate,
        DrawByRepetition,
        DrawByInactivity,
    }
}
