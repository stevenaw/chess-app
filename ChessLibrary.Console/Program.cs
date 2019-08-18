using ChessLibrary.Models;
using System;
using System.Text;

namespace ChessLibrary.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var game = new Game();
            var keepPlaying = true;
            BoardRenderer.PrintBoard(game);

            do
            {
                var turn = game.GetTurn();
                var cmd = GetCommand(turn);
                keepPlaying = ExecuteCommand(game, cmd);

                Console.WriteLine();
            } while (keepPlaying);
        }

        private static Command GetCommand(PieceColor turn)
        {
            Console.Write($"Enter command ({turn}'s turn): ");
            var input = Console.ReadLine().Trim().ToLower();

            var endOfCommandName = input.IndexOf(' ');
            var commandName = endOfCommandName == -1 ? input : input.Substring(0, endOfCommandName).ToLower();
            var commandArgs = (endOfCommandName == -1 || endOfCommandName == input.Length - 1) ? string.Empty : input.Substring(endOfCommandName + 1);

            return new Command()
            {
                TotalInput = input,
                CommandName = commandName,
                CommandArgs = commandArgs
            };
        }

        private static bool ExecuteCommand(Game game, Command cmd)
        {
            switch (cmd.CommandName)
            {
                case Commands.Prompt:
                    {
                        if (string.IsNullOrEmpty(cmd.CommandArgs))
                        {
                            Console.WriteLine("Please enter a square (ex: 'prompt a4')");
                            BoardRenderer.PrintBoard(game);
                        }
                        else
                        {
                            var sq = Game.ParseSquare(cmd.CommandArgs);
                            var moves = game.GetValidMoves(sq.File, sq.Rank);
                            BoardRenderer.PrintBoard(game, moves);
                        }

                        return true;
                    }

                case Commands.Exit:
                    {
                        Console.Write("Are you sure (Y\\N) ?: ");
                        return Console.ReadLine().ToLower() != "y";
                    }

                case Commands.Help:
                    {
                        Console.WriteLine();
                        Console.WriteLine("List of commands:");
                        foreach (var kvp in Commands.Descriptions)
                            Console.WriteLine($"- {kvp.Key}: {kvp.Value}");

                        Console.WriteLine("Not entering a command is assumed to be a move");
                        BoardRenderer.PrintBoard(game);

                        return true;
                    }

                case Commands.Move:
                    {
                        PerformMove(game, cmd.CommandArgs);
                        return true;
                    }

                default:
                    {
                        // By default, treat no command as a move
                        PerformMove(game, cmd.TotalInput);
                        return true;
                    }
            }
        }

        private static void PerformMove(Game game, string move)
        {
            var errorCode = game.Move(move);
            if (errorCode != 0)
            {
                // TODO: Better error reporting
                Console.WriteLine($"Invalid move ({errorCode}), please try again");
            }
            BoardRenderer.PrintBoard(game);
        }
    }
}
