namespace ChessLibrary.Models
{
    internal readonly struct GameState
    {
        public readonly BoardState BoardState { get; }
        public readonly AttackState AttackState { get; }
        public readonly Move PrecedingMove { get; }

        public GameState(BoardState board, AttackState attackState, Move previousMove)
        {
            BoardState = board;
            AttackState = attackState;
            PrecedingMove = previousMove;
        }

        public GameState SetAttackState(AttackState state)
        {
            return new GameState(BoardState, state, PrecedingMove);
        }

        public static GameState Initialize(BoardState boardState)
        {
            return new GameState(boardState, AttackState.None, Move.Empty);
        }

        public static GameState FromState(BoardState boardState, AttackState attackState, Move precedingMove)
        {
            return new GameState(boardState, attackState, precedingMove);
        }
    }
}
