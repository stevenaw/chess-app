namespace ChessLibrary.Models
{
    public enum ErrorCondition : byte
    {
        None = 0,
        InvalidInput,
        InvalidSquare,
        InvalidMovement,
        MustMoveOwnPiece,
        CantTakeOwnPiece,
        PieceInWay,
        PiecePinned
    }
}
