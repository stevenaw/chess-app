using ChessLibrary.Models;
using ChessLibrary.Serialization;
using ChessLibrary.Tests.TestData;
using ChessLibrary.Tests.TestHelpers;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ChessLibrary.Tests
{
    [TestFixture]
    public class PGNConverterTests
    {
        [Test]
        [TestCaseSource(nameof(PgnScenarios))]
        public async Task ReplayedGameMatchesExpectedFEN(string scenario)
        {
            var pgnStr = await ResourceHelpers.GetEmbeddedPGN(scenario);
            var pgnSerializer = new PGNSerializer();
            var pgn = await pgnSerializer.Deserialize(new StringReader(pgnStr));

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
            var pgnStr = await ResourceHelpers.GetEmbeddedPGN(scenario);
            var pgnSerializer = new PGNSerializer();
            var pgn = await pgnSerializer.Deserialize(new StringReader(pgnStr));

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
