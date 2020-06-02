using System;
using System.Collections.Generic;
using System.Text;

namespace ChessLibrary.Models
{
    internal class GameState
    {
        public BoardState BoardState { get; private set; }
        public AttackState AttackState { get; private set; }
        public Move? PrecedingMove { get; private set; }

        // TODO: A better way
        public bool HasWhiteCastled { get; private set; }
        public bool HasBlackCastled { get; private set; }

        public static GameState FromState(BoardState boardState, AttackState attackState)
        {
            return new GameState()
            {
                BoardState = boardState,
                AttackState = attackState,
                PrecedingMove = null,
                HasBlackCastled = false,
                HasWhiteCastled = false
            };
        }

        public static GameState FromState(BoardState boardState, AttackState attackState, Move precedingMove)
        {
            return new GameState()
            {
                BoardState = boardState,
                AttackState = attackState,
                PrecedingMove = precedingMove,
                HasBlackCastled = false,
                HasWhiteCastled = false
            };
        }
    }
}
