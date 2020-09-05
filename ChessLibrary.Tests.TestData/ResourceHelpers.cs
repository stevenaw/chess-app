using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace ChessLibrary.Tests.TestHelpers
{
    public static class ResourceHelpers
    {
        public static async Task<string> GetEmbeddedPGNString(string scenario)
        {
            using var s = GetEmbeddedPGNStream(scenario);
            using var sr = new StreamReader(s);

            return await sr.ReadToEndAsync();
        }

        public static Stream GetEmbeddedPGNStream(string scenario)
        {
            var resourceName = $"ChessLibrary.Tests.TestData.Games.{scenario}.pgn";
            var s = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName);

            return s ?? Stream.Null;
        }
    }
}
