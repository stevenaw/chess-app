using ChessLibrary.ConsoleApp.Rendering;
using ChessLibrary.Models;
using System;

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
            var input = Console.ReadLine().Trim();

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
                case Commands.Hint:
                    {
                        if (string.IsNullOrEmpty(cmd.CommandArgs))
                        {
                            Console.WriteLine("Please enter a square (ex: 'hint a4')");
                            BoardRenderer.PrintBoard(game);
                        }
                        else
                        {
                            var sq = Game.ParseSquare(cmd.CommandArgs);
                            var moves = game.GetValidMoves(sq.File, sq.Rank);
                            if (moves.Count == 0)
                                Console.WriteLine("No valid moves for that square");

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
                        return game.AttackState != AttackState.Checkmate;
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
                var message = GetErrorMessage(errorCode);
                Console.WriteLine($"{message}, please try again");
            }

            BoardRenderer.PrintBoard(game);
            if (game.AttackState != AttackState.None)
            {
                Console.Write(game.AttackState.ToString());
                Console.WriteLine("!");
            }
        }

        private static string GetErrorMessage(ErrorConditions error)
        {
            if (error == 0)
                return string.Empty;

            switch (error)
            {
                case ErrorConditions.CantTakeOwnPiece:
                    return "Can't take own piece";
                case ErrorConditions.InvalidInput:
                    return "Invalid Input";
                case ErrorConditions.InvalidMovement:
                    return "Invalid Movement";
                case ErrorConditions.InvalidSquare:
                    return "Invalid square";
                case ErrorConditions.MustMoveOwnPiece:
                    return "Must move own piece";
                case ErrorConditions.PieceInWay:
                    return "Piece in way";
                case ErrorConditions.PiecePinned:
                    return "Piece pinned";

                default:
                    return error.ToString();
            }
        }
    }
}
