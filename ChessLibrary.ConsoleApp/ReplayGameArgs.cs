using System;
using System.Linq;

namespace ChessLibrary.ConsoleApp
{
    internal readonly struct ReplayGameArgs
    {
        public readonly string FileName;
        public readonly TimeSpan MoveDelay;

        public ReplayGameArgs(string fileName, TimeSpan moveDelay)
        {
            FileName = fileName;
            MoveDelay = moveDelay;
        }

        public static ReplayGameArgs FromCliArgs(string[] args)
        {
            if (!args.Any())
                throw new ArgumentException("Please specify a PGN file to replay.");

            var file = args[0];
            var turnDelay = 1D;

            if (args.Length > 1)
            {
                if (!Double.TryParse(args[1], out turnDelay) || turnDelay <= 0)
                    throw new ArgumentException("Invalid turn delay specified. Must be greater than 0.");
            }

            return new ReplayGameArgs(file, TimeSpan.FromSeconds(turnDelay));
        }
    }
}
