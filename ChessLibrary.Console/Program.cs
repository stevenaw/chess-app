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

            do
            {
                var turn = game.GetTurn();

                PrintBoard(game);
                ExecuteCommand(game, turn);

                Console.WriteLine();
            } while (true);
        }

        private static void ExecuteCommand(Game game, PieceColor turn)
        {
            Console.Write($"Enter command ({turn}'s turn): ");
            var input = Console.ReadLine();

            var endOfCommandName = input.IndexOf(' ');

            var commandName = endOfCommandName == -1 ? "" : input.Substring(0, endOfCommandName).ToLower();
            var commandArgs = (endOfCommandName == -1 || endOfCommandName == input.Length - 1) ? input : input.Substring(endOfCommandName + 1);

            switch (commandName)
            {
                // TODO: More commands
                // prompt a2 -> gives possible moves for a2
                // exit -> exits
                case "move":
                    {
                        PerformMove(game, commandArgs);
                        return;
                    }

                default:
                    {
                        // By default, treat no command as a move
                        PerformMove(game, input);
                        break;
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

        private static void PrintBoard(Game game)
        {
            const string rankDivider = "  -----------------";

            // This could instead just be a char[]
            // At 2 EOL markers (\r\n) it's 380 in length
            var output = new StringBuilder(
                // row = 8 squares + row number. Each has a delimiter (so *2)
                ((8 + 1) * 2 + Environment.NewLine.Length + 1)
                // rows to output = 8 squares + delimiter for each
                * (8 + 1) * 2
                 + Environment.NewLine.Length
            );

            output.Append(rankDivider);
            output.Append(Environment.NewLine);

            for (var rank = 8; rank > 0; rank--)
            {
                output.Append((char)(rank + '0'));
                output.Append(" |");

                for (var file = 'a'; file <= 'h'; file++)
                {
                    var contents = game.GetSquareContents(file, rank);
                    var representation = GetPieceRepresentation(contents);

                    output.Append(representation);
                    output.Append('|');
                }

                output.Append(Environment.NewLine);
                output.Append(rankDivider);
                output.Append(Environment.NewLine);
            }

            output.Append("  ");
            for (var file = 'A'; file <= 'H'; file++)
            {
                output.Append(' ');
                output.Append(file);
            }
            output.Append(Environment.NewLine);
            output.Append(Environment.NewLine);

            Console.Write(output.ToString());
        }

        private static char GetPieceRepresentation(SquareContents contents)
        {
            const char UpperDiff = (char)('a' - 'A');

            if (contents == 0)
                return ' ';

            char result = '\0';
            switch (contents & SquareContents.Pieces)
            {
                case SquareContents.King:
                    result = 'K';
                    break;
                case SquareContents.Queen:
                    result = 'Q';
                    break;
                case SquareContents.Rook:
                    result = 'R';
                    break;
                case SquareContents.Bishop:
                    result = 'B';
                    break;
                case SquareContents.Knight:
                    result = 'N';
                    break;
                case SquareContents.Pawn:
                    result = 'P';
                    break;
            }

            if (contents.HasFlag(SquareContents.Black))
                result += UpperDiff;

            return result;
        }
    }
}
