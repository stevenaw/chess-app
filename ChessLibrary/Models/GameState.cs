namespace ChessLibrary.Models
{
    // TODO: Immutable
    internal sealed class GameState
    {
        public BoardState BoardState { get; private set; }
        public AttackState AttackState { get; private set; }
        public Move PrecedingMove { get; private set; }

        public void SetAttackState(AttackState state)
        {
            AttackState = state;
        }

        public static GameState Initialize(BoardState boardState)
        {
            return new GameState()
            {
                BoardState = boardState,
                AttackState = AttackState.None,
                PrecedingMove = Move.Empty
            };
        }

        public static GameState FromState(BoardState boardState, AttackState attackState, Move precedingMove)
        {
            return new GameState()
            {
                BoardState = boardState,
                AttackState = attackState,
                PrecedingMove = precedingMove
            };
        }
    }
}
