using ChessLibrary.Models;
using ChessLibrary.Serialization;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace ChessLibrary.Tests
{
    [TestFixture]
    public class GameTests
    {
        [Test]
        public void EnPassant_IsAllowed_ImmediatelyAfterPush()
        {
            var game = new Game();
            var moveStrings = new[]
            {
                "e4", "Na6", "e5", "d5", "exd6"
            };

            foreach (var moveString in moveStrings)
            {
                var move = game.ParseMove(moveString);
                var result = game.Move(move);
                Assert.That(result, Is.EqualTo(ErrorCondition.None));
            }

            Assert.That(game.GetSquareContents('d', 6), Is.EqualTo(SquareContents.Pawn | SquareContents.White));
            Assert.That(game.GetSquareContents('d', 5), Is.EqualTo(SquareContents.Empty));
        }


        [Test]
        public void EnPassant_IsDisallowed_WhenNotImmediatelyAfterPush()
        {
            var game = new Game();
            var moveStrings = new[]
            {
                "e4", "Na6", "e5", "d5", // Setup
                "Na3", "Nb8", // Wait a bit
                "exd6" // Attempt
            };

            for (var i = 0; i < moveStrings.Length - 1; i++)
            {
                var move = game.ParseMove(moveStrings[i]);
                var result = game.Move(move);
                Assert.That(result, Is.EqualTo(ErrorCondition.None));
            }

            {
                var move = game.ParseMove(moveStrings.Last());
                var result = game.Move(move);
                Assert.That(result, Is.EqualTo(ErrorCondition.InvalidMovement));
            }
        }

        [Test]
        public void EnPassant_IsDisallowed_WhenNotPushed()
        {
            var game = new Game();
            var moveStrings = new[]
            {
                "e4", "Na6", "e5", "d6", "Na3", "d5", "exd6"
            };

            for(var i = 0; i < moveStrings.Length-1; i++)
            {
                var move = game.ParseMove(moveStrings[i]);
                var result = game.Move(move);
                Assert.That(result, Is.EqualTo(ErrorCondition.None));
            }

            {
                var move = game.ParseMove(moveStrings.Last());
                var result = game.Move(move);
                Assert.That(result, Is.EqualTo(ErrorCondition.InvalidMovement));
            }
        }

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

            Assert.That(result, Is.EqualTo(ErrorCondition.None));
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
                "Nb1", "Nb8",
                "Nc3"
            };

            for (var i = 0; i < moves.Length-1; i++)
            {
                game.Move(moves[i]);
                Assert.That(game.AttackState, Is.Not.EqualTo(AttackState.DrawByRepetition));
            }

            game.Move(moves.Last());
            Assert.That(game.AttackState, Is.EqualTo(AttackState.DrawByRepetition));
        }

        [Test]
        public void Move_DetectsDrawByInactivity()
        {
            var game = new Game();
            var setupSteps = new string[]
            {
                "h3", "h6",
            };
            var mainTour = new string[]
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
            var interjectionSteps = new[]
            {
                "Rh2", "Rh7",
                "Rh1", "Rh8",
            };

            var moves = new List<string>();
            for (var i = 0; i < Constants.MoveLimits.RepetitionLimit; i++)
            {
                for (var j = 0; j < interjectionSteps.Length; j += 2)
                {
                    moves.AddRange(mainTour);
                    moves.Add(interjectionSteps[j]);
                    moves.Add(interjectionSteps[j + 1]);
                }
            }

            Assume.That(moves.Count, Is.GreaterThanOrEqualTo(Constants.MoveLimits.InactivityLimit));

            foreach (var setupStep in setupSteps)
                game.Move(setupStep);

            for (var i = 0; i < Constants.MoveLimits.InactivityLimit-1; i++)
            {
                Assert.That(
                    game.AttackState,
                    Is.EqualTo(AttackState.None),
                    () => $"Move {(i-1)/2D} of '{moves[i-1]}' resulted in {game.AttackState}"
                );

                var result = game.Move(moves[i]);
                Assume.That(result, Is.EqualTo(ErrorCondition.None));
            }

            Assert.That(game.AttackState, Is.EqualTo(AttackState.DrawByInactivity));
        }
    }
}
