using ChessLibrary.Models;
using ChessLibrary.Serialization;
using ChessLibrary.Tests.TestData;
using ChessLibrary.Tests.TestHelpers;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ChessLibrary.Tests
{
    [TestFixture]
    class PGNSerializerTests
    {
        [Test]
        [TestCaseSource(nameof(NoAnnotations))]
        public async Task SerializeNoAnnotations_IgnoreLineEndings((string scenario, PGNMetadata metadata) args)
        {
            (string scenario, PGNMetadata pgn) = args;

            var serializer = new PGNSerializer();
            var result = string.Empty;
            using (var writer = new StringWriter())
            {
                await serializer.SerializeAsync(pgn, writer);
                result = writer.ToString();
            }

            var expectedResult = await ResourceHelpers.GetEmbeddedPGNString(scenario);

            Assert.That(result, Is.EqualTo(expectedResult));
        }

        [Test]
        [TestCaseSource(nameof(ParsingScenarios))]
        public async Task Deserialize_ParsesMainTags((string scenario, PGNMetadata metadata) args)
        {
            (string scenario, PGNMetadata expected) = args;

            var fileContents = await ResourceHelpers.GetEmbeddedPGNString(scenario);
            var serializer = new PGNSerializer();
            PGNMetadata actual;

            using (var reader = new StringReader(fileContents))
            {
                actual = await serializer.DeserializeAsync(reader);
            }

            Assert.That(actual.White, Is.EqualTo(expected.White));
            Assert.That(actual.Black, Is.EqualTo(expected.Black));
            Assert.That(actual.Date, Is.EqualTo(expected.Date));
            Assert.That(actual.Result, Is.EqualTo(expected.Result));
            Assert.That(actual.Event, Is.EqualTo(expected.Event));
            Assert.That(actual.Round, Is.EqualTo(expected.Round));
            Assert.That(actual.Site, Is.EqualTo(expected.Site));
        }

        [TestCaseSource(nameof(ParsingScenarios))]
        public async Task Deserialize_ParsesMoves((string scenario, PGNMetadata metadata) args)
        {
            (string scenario, PGNMetadata expected) = args;

            var fileContents = await ResourceHelpers.GetEmbeddedPGNString(scenario);
            var serializer = new PGNSerializer();
            PGNMetadata actual;

            using (var reader = new StringReader(fileContents))
            {
                actual = await serializer.DeserializeAsync(reader);
            }

            Assert.That(actual.Moves, Is.EqualTo(expected.Moves));
        }


        [TestCaseSource(nameof(ParsingScenarios))]
        public async Task RoundTrip_Serialization((string scenario, PGNMetadata metadata) args)
        {
            (string scenario, PGNMetadata expected) = args;

            var fileContents = await ResourceHelpers.GetEmbeddedPGNString(scenario);
            var serializer = new PGNSerializer();
            PGNMetadata actualMetadata;

            using (var reader = new StringReader(fileContents))
            {
                actualMetadata = await serializer.DeserializeAsync(reader);
            }

            var actualGame = PGNConverter.ConvertToGame(actualMetadata);
            var backToMetadata = PGNConverter.ConvertFromGame(actualGame);

            Assert.That(backToMetadata.Moves, Is.EqualTo(expected.Moves));
        }


        private static IEnumerable<(string scenario, PGNMetadata metadata)> ParsingScenarios
        {
            get
            {
                // Basic game
                var scenario = Scenarios.Fischer_Spassky_1992_Game29;
                var metadata = new PGNMetadata()
                {
                    Event = "F/S Return Match",
                    Site = "Belgrade, Serbia JUG",
                    Date = "1992.11.04",
                    Round = "29",
                    Result = "1/2-1/2",
                    White = "Fischer, Robert J.",
                    Black = "Spassky, Boris V.",
                    Moves = @"e4,e5,Nf3,Nc6,Bb5,a6,Ba4,Nf6,O-O,Be7,Re1,b5,Bb3,d6,c3
                    ,O-O,h3,Nb8,d4,Nbd7,c4,c6,cxb5,axb5,Nc3,Bb7,Bg5,b4
                    ,Nb1,h6,Bh4,c5,dxe5,Nxe4,Bxe7,Qxe7,exd6,Qf6,Nbd2,Nxd6
                    ,Nc4,Nxc4,Bxc4,Nb6,Ne5,Rae8,Bxf7+,Rxf7,Nxf7,Rxe1+,Qxe1
                    ,Kxf7,Qe3,Qg5,Qxg5,hxg5,b3,Ke6,a3,Kd6,axb4,cxb4,Ra5,Nd5
                    ,f3,Bc8,Kf2,Bf5,Ra7,g6,Ra6+,Kc5,Ke1,Nf4,g3,Nxh3,Kd2
                    ,Kb5,Rd6,Kc5,Ra6,Nf2,g4,Bd3,Re6".Split(',').Select(o => o.Trim()).ToList()
                };

                yield return (scenario, metadata);

                // Multi-line annotations
                scenario = Scenarios.Fischer_Byrne_1956;
                metadata = new PGNMetadata()
                {
                    Event = "Third Rosenwald Trophy",
                    Site = "New York, NY USA",
                    Date = "1956.10.17",
                    Round = "8",
                    Result = "0-1",
                    White = "Donald Byrne",
                    Black = "Robert James Fischer",
                    Moves = @"Nf3,Nf6,c4,g6,Nc3,Bg7,d4,O-O,Bf4,d5,Qb3
                            ,dxc4,Qxc4,c6,e4,Nbd7,Rd1,Nb6,Qc5,Bg4,Bg5,Na4,Qa3,Nxc3,bxc3,Nxe4
                            ,Bxe7,Qb6,Bc4,Nxc3,Bc5,Rfe8+,Kf1,Be6,Bxb6,Bxc4+
                            ,Kg1,Ne2+,Kf1,Nxd4+,Kg1,Ne2+,Kf1,Nc3+,Kg1,axb6,Qb4
                            ,Ra4,Qxb6,Nxd1,h3,Rxa2,Kh2,Nxf2,Re1,Rxe1
                            ,Qd8+,Bf8,Nxe1,Bd5,Nf3,Ne4,Qb8,b5,h4,h5,Ne5,Kg7,Kg1,Bc5+,Kf1
                            ,Ng3+,Ke1,Bb4+,Kd1,Bb3+,Kc1,Ne2+,Kb1,Nc3+
                            ,Kc1,Rc2#".Split(',').Select(o => o.Trim()).ToList()
                };

                yield return (scenario, metadata);

                // Full-line annotation
                scenario = Scenarios.Fischer_Altusky_1954;
                metadata = new PGNMetadata()
                {
                    Event = "Offhand Game",
                    Site = "New York, NY USA",
                    Date = "1954.12.??",
                    Round = "?",
                    Result = "0-1",
                    White = "Jacob Altusky",
                    Black = "Robert James Fischer",
                    Moves = @"e4,e5,Nf3,Nc6,Bb5,a6,Ba4,d6,d4,b5,Bb3,Bg4,Bxf7+,Kxf7,Ng5+,Qxg5".Split(',').Select(o => o.Trim()).ToList()
                };

                yield return (scenario, metadata);
            }
        }

        private static IEnumerable<(string scenario, PGNMetadata metadata)> NoAnnotations
        {
            get
            {
                var scenario = Scenarios.Fischer_Spassky_1992_Game29;
                var metadata = new PGNMetadata()
                {
                    Event = "F/S Return Match",
                    Site = "Belgrade, Serbia JUG",
                    Date = "1992.11.04",
                    Round = "29",
                    White = "Fischer, Robert J.",
                    Black = "Spassky, Boris V.",
                    Result = "1/2-1/2",
                    Moves = @"e4,e5,Nf3,Nc6,Bb5,a6,Ba4,Nf6,O-O,Be7,Re1,b5,Bb3,d6,c3
                    ,O-O,h3,Nb8,d4,Nbd7,c4,c6,cxb5,axb5,Nc3,Bb7,Bg5,b4
                    ,Nb1,h6,Bh4,c5,dxe5,Nxe4,Bxe7,Qxe7,exd6,Qf6,Nbd2,Nxd6
                    ,Nc4,Nxc4,Bxc4,Nb6,Ne5,Rae8,Bxf7+,Rxf7,Nxf7,Rxe1+,Qxe1
                    ,Kxf7,Qe3,Qg5,Qxg5,hxg5,b3,Ke6,a3,Kd6,axb4,cxb4,Ra5,Nd5
                    ,f3,Bc8,Kf2,Bf5,Ra7,g6,Ra6+,Kc5,Ke1,Nf4,g3,Nxh3,Kd2
                    ,Kb5,Rd6,Kc5,Ra6,Nf2,g4,Bd3,Re6".Split(',').Select(o => o.Trim()).ToList()
                };

                yield return (scenario, metadata);
            }
        }
    }
}
