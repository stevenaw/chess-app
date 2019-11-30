using ChessLibrary.Models;
using ChessLibrary.Serialization;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChessLibrary.Tests
{
    [TestFixture]
    public class GameTests
    {
        [TestCase("a8=Q", "a7", "a8", PieceColor.White, SquareContents.White | SquareContents.Queen)]
        [TestCase("a1=Q", "a2", "a1", PieceColor.Black, SquareContents.Black | SquareContents.Queen)]
        public void TryParseMove_ParsesPromotion(string input, string expectedStart, string expectedEnd, PieceColor color, SquareContents piece)
        {
            var endSq = MoveParser.ParseSquare(expectedEnd);
            var startSq = MoveParser.ParseSquare(expectedStart);
            var pieceColor = color == PieceColor.White ? SquareContents.White : SquareContents.Black;
            var board = BoardState.Empty.SetPiece(startSq, SquareContents.Pawn | pieceColor);

            var game = new Game(board, color);
            var result = game.Move(input);
            var expectedSquare = game.GetSquareContents(endSq.File, endSq.Rank);

            Assert.That(result, Is.EqualTo(ErrorConditions.None));
            Assert.That(expectedSquare, Is.EqualTo(piece));
        }

        [TestCase("k7/8/8/2b5/8/8/p7/Kb6", "Bd4", ExpectedResult = AttackState.Checkmate)]
        [TestCase("k7/8/8/2b5/8/8/pR6/Kb6", "Bd4", ExpectedResult = AttackState.Stalemate)]
        [TestCase("k7/8/8/2b5/8/8/pB6/Kb6", "Bd4", ExpectedResult = AttackState.None)]
        public AttackState Move_DetectsAttackState(string position, string move)
        {
            var fen = new FenSerializer();
            var board = fen.Deserialize(position);
            var game = new Game(board, PieceColor.Black);

            game.Move(move);

            return game.AttackState;
        }

        [Test]
        public void Move_DetectsDrawByRepetition()
        {
            var game = new Game();
            var moves = new string[]
            {
                "Nc3", "Nc6",
                "Nb1", "Nb8",
                "Nc3", "Nc6",
                "Nb1", "Nb8"
            };

            foreach (var move in moves)
            {
                Assert.That(game.AttackState, Is.Not.EqualTo(AttackState.DrawByRepetition));
                game.Move(move);
            }

            Assert.That(game.AttackState, Is.EqualTo(AttackState.DrawByRepetition));
        }

        [Test]
        public void Move_DetectsDrawByInactivity()
        {
            // TODO: This accidentally counts the start position in the test. We shouldn't do that
            var game = new Game();
            var tourSteps = new string[]
            {
                // A cycle of moves to repeat
                "Nc3", "Nc6",
                "Na4", "Na5",
                "Nc5", "Nc4",
                "Na6", "Na3",
                "Nb4", "Nb5",
                "Nc6", "Nc3",
                "Ne5", "Ne4",
                "Nd3", "Nd6",
                "Nf4", "Nf5",
                "Nh5", "Nh4",
                "Ng3", "Ng6",
                "Nf5", "Nf4",
                "Nd4", "Nd5",
                "Ne6", "Ne3",
                "Ng5", "Ng4",
                "Ne4", "Ne5",
            };
            var moves = Enumerable.Range(1, 3).SelectMany(i => tourSteps).ToArray();

            Assume.That(moves.Length, Is.GreaterThan(Constants.MoveLimits.InactivityLimit));

            for(var i = 0; i < Constants.MoveLimits.InactivityLimit; i++)
            {
                Assert.That(game.AttackState, Is.EqualTo(AttackState.None));
                game.Move(moves[i]);
            }

            Assert.That(game.AttackState, Is.EqualTo(AttackState.DrawByInactivity));
        }
    }
}
