using ChessLibrary.Models;
using ChessLibrary.Serialization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ChessLibrary.ConsoleApp
{
    public class ReplayGame
    {
        private readonly string _file;
        private readonly int _msPerMove;

        public ReplayGame(string file, int msPerMove)
        {
            _file = file;
            _msPerMove = msPerMove;
        }

        public async Task Run()
        {
            var pgn = await GetPGN(_file);
            var game = new Game();

            if (pgn.Moves.Any())
            {
                BoardRenderer.PrintBoard(game);

                foreach(var move in pgn.Moves)
                {
                    await Task.Delay(_msPerMove);
                    game.Move(move);
                    BoardRenderer.PrintBoard(game);
                }
            }
        }

        private static async Task<PGNMetadata> GetPGN(string fileName)
        {
            var serializer = new PGNSerializer();

            using var file = File.OpenRead(fileName);
            using var reader = new StreamReader(file);

            return await serializer.DeserializeAsync(reader);
        }
    }
}
