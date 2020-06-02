namespace ChessLibrary.Models
{
    public enum ErrorCondition
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
