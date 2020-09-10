using System;
using System.Linq;
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
                case "replay":
                    {
                        // TODO: Better error reporting, move arg validation into "ReplayGame"
                        var file = args[1];
                        int turnDelay = 1000;
                        if (args.Length > 2 && !int.TryParse(args[2], out turnDelay))
                            turnDelay = 1000;

                        var game = new ReplayGame(file, turnDelay);
                        
                        await game.Run();
                        break;
                    }

                case "play":
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
