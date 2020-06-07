namespace ChessLibrary.Models
{
    public enum MoveAnnotation : byte
    {
        Normal,
        Excellent,      //  !
        Brilliancy,     // !!
        Mistake,        //  ?
        Blunder,        // ??
        Interesting,    // !?
        Dubious         // ?!
    }
}
