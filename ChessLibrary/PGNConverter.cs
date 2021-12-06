using ChessLibrary.Models;

namespace ChessLibrary
{
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

        private static string[] GetMoves(Game game)
        {
            var history = game.History.ToArray();
            Array.Reverse(history);

            var moves = new string[history.Length - 1];
            for (var i = 1; i < history.Length; i++)
            {
                var move = history[i].PrecedingMove;
                var board = history[i - 1].Board;
                var result = history[i].AttackState;

                var moveStr = MoveParser.ToMoveString(move, board, result);
                if (moveStr.StartsWith('0'))
                    moveStr = moveStr.Replace('0', 'O');

                moves[i - 1] = moveStr;
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
