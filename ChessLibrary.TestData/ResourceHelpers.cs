using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace ChessLibrary.Tests.TestHelpers
{
    public static class ResourceHelpers
    {
        public static async Task<string> GetEmbeddedPGN(string scenario)
        {
            var resourceName = $"ChessLibrary.Tests.TestData.Games.{scenario}.pgn";
            using var s = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);
            if (s == null)
                return string.Empty;

            using var sr = new StreamReader(s);
            return await sr.ReadToEndAsync();
        }
    }
}
