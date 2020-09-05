using ChessLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChessLibrary
{
    // TODO: Tests
    public static class PGNConverter
    {
        public static PGNMetadata ConvertFromGame(Game game)
        {
            var pgn = new PGNMetadata();

            pgn.Moves = GetMoves(game);
            pgn.Result = GetResult(game);

            return pgn;
        }

        public static Game ConvertToGame(PGNMetadata pgn)
        {
            var game = new Game();

            foreach (var move in pgn.Moves)
                game.Move(move);

            return game;
        }

        private static List<string> GetMoves(Game game)
        {
            // TODO: Better way to reverse
            var history = game.History.ToArray().Reverse().ToArray();

            var moves = new List<string>(history.Length - 1);
            for (var i = 0; i < history.Length - 1; i++)
            {
                var move = history[i + 1].PrecedingMove;
                var board = history[i].Board;
                var result = history[i + 1].AttackState;

                var moveStr = MoveParser.ToMoveString(move, board, result);
                if (moveStr.StartsWith('0'))
                    moveStr = moveStr.Replace('0', 'O');

                moves[i] = moveStr;
            }

            return moves;
        }

        private static string GetResult(Game game)
        {
            switch (game.AttackState)
            {
                case AttackState.DrawByInactivity:
                case AttackState.DrawByRepetition:
                case AttackState.Stalemate:
                    return "1/2-1/2";
                case AttackState.Checkmate:
                    return game.GetTurn() == PieceColor.Black ? "1-0" : "0-1";
                case AttackState.None:
                case AttackState.Check:
                default:
                    return "*";
            }
        }
    }
}
