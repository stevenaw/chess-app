using ChessLibrary.Models;
using System;

namespace ChessLibrary.ConsoleApp
{
    public class InteractiveGame
    {
        public void Run()
        {
            var game = new Game();
            bool keepPlaying;
            BoardRenderer.PrintBoard(game);

            do
            {
                var turn = game.GetTurn();
                var cmd = GetCommand(turn);

                keepPlaying = ExecuteCommand(game, cmd);

                Console.WriteLine();
            } while (keepPlaying);
        }

        private static InteractiveGameCommand GetCommand(PieceColor turn)
        {
            Console.Write($"Enter command ({turn}'s turn): ");
            var input = (Console.ReadLine() ?? string.Empty).Trim();

            var endOfCommandName = input.IndexOf(' ');
            var commandName = endOfCommandName == -1 ? input : input.Substring(0, endOfCommandName).ToLower();
            var commandArgs = (endOfCommandName == -1 || endOfCommandName == input.Length - 1) ? string.Empty : input.Substring(endOfCommandName + 1);

            return new InteractiveGameCommand(input, commandName, commandArgs);
        }

        private static bool ExecuteCommand(Game game, InteractiveGameCommand cmd)
        {
            switch (cmd.CommandName)
            {
                case InteractiveGameCommands.Hint:
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

                case InteractiveGameCommands.Exit:
                    {
                        Console.Write("Are you sure (Y\\N) ?: ");
                        return (Console.ReadLine() ?? string.Empty).ToLower() != "y";
                    }

                case InteractiveGameCommands.Help:
                    {
                        Console.WriteLine();
                        Console.WriteLine("List of commands:");
                        foreach (var kvp in InteractiveGameCommands.Descriptions)
                            Console.WriteLine($"- {kvp.Key}: {kvp.Value}");

                        Console.WriteLine("Not entering a command is assumed to be a move");
                        BoardRenderer.PrintBoard(game);

                        return true;
                    }

                case InteractiveGameCommands.Move:
                    {
                        PerformMove(game, cmd.CommandArgs);
                        return game.IsOngoing;
                    }

                case InteractiveGameCommands.Undo:
                    {
                        var success = game.Undo();
                        if (!success)
                            Console.WriteLine("Can't undo from this state");

                        BoardRenderer.PrintBoard(game);

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
                var message = GetErrorMessage(errorCode);
                Console.WriteLine($"{message}, please try again");
            }

            BoardRenderer.PrintBoard(game);
            if (game.AttackState != AttackState.None)
            {
                Console.Write(EnumHelper.GetAttackString(game.AttackState));
                Console.WriteLine("!");
            }
        }

        private static string GetErrorMessage(ErrorCondition error)
        {
            if (error == 0)
                return string.Empty;

            switch (error)
            {
                case ErrorCondition.CantTakeOwnPiece:
                    return "Can't take own piece";
                case ErrorCondition.InvalidInput:
                    return "Invalid input";
                case ErrorCondition.InvalidMovement:
                    return "Invalid movement";
                case ErrorCondition.InvalidSquare:
                    return "Invalid square";
                case ErrorCondition.MustMoveOwnPiece:
                    return "Must move own piece";
                case ErrorCondition.PieceInWay:
                    return "Piece in way";
                case ErrorCondition.PiecePinned:
                    return "Piece pinned";

                default:
                    return EnumHelper.GetErrorString(error);
            }
        }
    }
}
