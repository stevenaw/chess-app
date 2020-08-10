using ChessLibrary.Models;
using ChessLibrary.Serialization;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ChessLibrary.Tests
{
    [TestFixture]
    class PGNSerializerTests
    {
        [TestCaseSource(nameof(NoAnnotations))]
        [Test]
        public async Task SerializeNoAnnotations_IgnoreLineEndings((string scenario, string[] moves, PGNMetadata metadata) args)
        {
            (string scenario, string[] moves, PGNMetadata metadata) = args;
            var game = new Game();

            foreach(var move in moves)
            {
                var wasMoveError = game.Move(move);
                Assume.That(wasMoveError, Is.EqualTo(ErrorCondition.None));
            }

            if (string.IsNullOrEmpty(metadata.Result))
                metadata.Result = GetResult(game);

            metadata.Moves = GetMoves(game);

            var serializer = new PGNSerializer();
            var result = string.Empty;
            using (var writer = new StringWriter())
            {
                await serializer.Serialize(metadata, writer);
                result = writer.ToString();
            }

            var expectedResult = await GetEmbeddedPGN(scenario);

            // TODO: Account for line endings
            Assert.That(result.Replace(Environment.NewLine, " "), Is.EqualTo(expectedResult.Replace(Environment.NewLine, " ")));
        }

        private static async Task<string> GetEmbeddedPGN(string scenario)
        {
            var resourceName = $"ChessLibrary.Tests.Data.{scenario}.pgn";
            using (var s = Assembly.GetExecutingAssembly().GetManifestResourceStream(resourceName))
            {
                using (var sr = new StreamReader(s))
                    return await sr.ReadToEndAsync();
            }
        }

        // TODO: Convert for game <-> pgn models
        private static string[] GetMoves(Game game)
        {
            // TODO: Better way to reverse
            var history = game.History.ToArray().Reverse().ToArray();

            var moves = new string[history.Length - 1];
            for (var i = 0; i < history.Length - 1; i++)
            {
                var move = history[i + 1].PrecedingMove;
                var board = history[i].Board;
                var result = history[i + 1].AttackState;

                var moveStr = MoveParser.ToMoveString(move, board, result);
                if (moveStr.StartsWith('0'))
                    moveStr = moveStr.Replace('0', 'O');

                moves[i] = moveStr;
            }

            return moves;
        }
        private static string GetResult(Game game)
        {
            switch (game.AttackState)
            {
                case AttackState.DrawByInactivity:
                case AttackState.DrawByRepetition:
                case AttackState.Stalemate:
                    return "1/2-1/2";
                case AttackState.Checkmate:
                    return game.GetTurn() == PieceColor.Black ? "1-0" : "0-1";
                case AttackState.None:
                case AttackState.Check:
                default:
                    return "*";
            }
        }

        private static IEnumerable<(string scenario, string[] moves, PGNMetadata metadata)> NoAnnotations
        {
            get
            {
                var scenario = "Fischer_Spassky_1992_Game29";
                var metadata = new PGNMetadata()
                {
                    Event = "F/S Return Match",
                    Site = "Belgrade, Serbia JUG",
                    DateTime = new DateTime(1992, 11, 04),
                    Round = "29",
                    White = "Fischer, Robert J.",
                    Black = "Spassky, Boris V.",
                    Result = "1/2-1/2"
                };
                var moves = @"e4,e5,Nf3,Nc6,Bb5,a6,Ba4,Nf6,O-O,Be7,Re1,b5,Bb3,d6,c3
                    ,O-O,h3,Nb8,d4,Nbd7,c4,c6,cxb5,axb5,Nc3,Bb7,Bg5,b4
                    ,Nb1,h6,Bh4,c5,dxe5,Nxe4,Bxe7,Qxe7,exd6,Qf6,Nbd2,Nxd6
                    ,Nc4,Nxc4,Bxc4,Nb6,Ne5,Rae8,Bxf7+,Rxf7,Nxf7,Rxe1+,Qxe1
                    ,Kxf7,Qe3,Qg5,Qxg5,hxg5,b3,Ke6,a3,Kd6,axb4,cxb4,Ra5,Nd5
                    ,f3,Bc8,Kf2,Bf5,Ra7,g6,Ra6+,Kc5,Ke1,Nf4,g3,Nxh3,Kd2
                    ,Kb5,Rd6,Kc5,Ra6,Nf2,g4,Bd3,Re6".Split(',').Select(o => o.Trim()).ToArray();

                yield return (scenario, moves, metadata);
            }
        }
    }
}
