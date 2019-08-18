namespace ChessLibrary.Models
{
    public enum ErrorConditions
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
