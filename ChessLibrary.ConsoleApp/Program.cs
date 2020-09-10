﻿using System.Linq;
using System.Threading.Tasks;

namespace ChessLibrary.ConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var firstArg = (args.FirstOrDefault() ?? string.Empty).ToLower();
            switch (firstArg)
            {
                case Modes.Replay:
                    {
                        var replayArgs = ReplayGameArgs.FromCliArgs(args.Skip(1).ToArray());
                        var game = new ReplayGame(replayArgs.FileName, replayArgs.MsTurnDelay);
                        
                        await game.Run();
                        break;
                    }

                case Modes.Play:
                case "":
                    {
                        var game = new InteractiveGame();
                        game.Run();
                        break;
                    }
            }
        }
    }
}