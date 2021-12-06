using ChessLibrary.Models;
using ChessLibrary.Serialization;
using ChessLibrary.Tests.TestData;
using ChessLibrary.Tests.TestHelpers;
using NUnit.Framework;

namespace ChessLibrary.Tests
{
    [TestFixture]
    public class ReplayedGameTests
    {
        [Test]
        [TestCaseSource(nameof(PgnScenarios))]
        public async Task ReplayedGameMatchesExpectedFEN(string scenario)
        {
            var pgnSerializer = new PGNSerializer();
            using var stream = ResourceHelpers.GetEmbeddedPGNStream(scenario);
            var pgn = await pgnSerializer.DeserializeAsync(new StreamReader(stream));

            var expectedFen = Scenarios.FinalPositions[scenario];

            var game = new Game();
            foreach (var move in pgn.Moves)
            {
                var result = game.Move(move);
                Warn.If(result, Is.Not.EqualTo(ErrorCondition.None), $"Unexpected result for move {move}");
            }

            var fenSerializer = new FenSerializer();
            var actualFen = fenSerializer.Serialize(game.CurrentState.Board);

            Assert.That(actualFen, Is.EqualTo(expectedFen));
        }

        [Test]
        [TestCaseSource(typeof(Scenarios.MatingScenarios), nameof(Scenarios.MatingScenarios.All))]
        public async Task ReplayedGameEndsInCheckmate(string scenario)
        {
            var pgnSerializer = new PGNSerializer();
            using var stream = ResourceHelpers.GetEmbeddedPGNStream(scenario);
            var pgn = await pgnSerializer.DeserializeAsync(new StreamReader(stream));

            var game = new Game();
            foreach (var move in pgn.Moves)
            {
                var result = game.Move(move);
                Warn.If(result, Is.Not.EqualTo(ErrorCondition.None), $"Unexpected result for move {move}");
            }

            Assert.That(game.AttackState, Is.EqualTo(AttackState.Checkmate));
        }

        public static IEnumerable<string> PgnScenarios
        {
            get => Scenarios.FinalPositions.Keys;
        }
    }
}
