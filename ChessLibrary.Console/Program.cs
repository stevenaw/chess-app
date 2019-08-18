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

            do
            {
                var turn = game.GetTurn();

                BoardRenderer.PrintBoard(game);
                keepPlaying = ExecuteCommand(game, turn);

                Console.WriteLine();
            } while (keepPlaying);
        }

        private static bool ExecuteCommand(Game game, PieceColor turn)
        {
            Console.Write($"Enter command ({turn}'s turn): ");
            var input = Console.ReadLine().Trim().ToLower();

            var endOfCommandName = input.IndexOf(' ');

            var commandName = endOfCommandName == -1 ? input : input.Substring(0, endOfCommandName).ToLower();
            var commandArgs = (endOfCommandName == -1 || endOfCommandName == input.Length - 1) ? input : input.Substring(endOfCommandName + 1);

            switch (commandName)
            {
                // TODO: More commands
                // - prompt a2 -> gives possible moves for a2
                case Commands.Move:
                    {
                        PerformMove(game, commandArgs);
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
                        return true;
                    }

                default:
                    {
                        // By default, treat no command as a move
                        PerformMove(game, input);
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
        }
    }
}
