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
                        var game = ReplayGame.FromArgs(args);
                        if (game != null)
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
