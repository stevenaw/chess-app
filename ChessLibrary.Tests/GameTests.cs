using ChessLibrary.Models;
using ChessLibrary.Serialization;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace ChessLibrary.Tests
{
    //TODO: Promotion tests
    [TestFixture]
    public class GameTests
    {
        [Test]
        public void EnPassant_UpdatesState_WhenAllowed()
        {
            var game = new Game();
            var moveStrings = new[]
            {
                "e4", "Na6", "e5", "d5", "exd6"
            };

            foreach (var moveString in moveStrings)
            {
                var result = game.Move(moveString);
                Assume.That(result, Is.EqualTo(ErrorCondition.None));
            }

            Assert.That(game.GetSquareContents('d', 6), Is.EqualTo(SquareContents.Pawn | SquareContents.White));
            Assert.That(game.GetSquareContents('d', 5), Is.EqualTo(SquareContents.Empty));
        }


        [Test]
        public void EnPassant_DoesntUpdateState_WhenDisallowed()
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
                var result = game.Move(moveStrings[i]);
                Assert.That(result, Is.EqualTo(ErrorCondition.None));
            }

            {
                var move = game.ParseMove(moveStrings.Last());
                var result = game.Move(move);
                Assert.That(result, Is.EqualTo(ErrorCondition.InvalidMovement));
            }
        }

        [Test]
        public void Move_UpdatesCastlingMovementTracker()
        {
            var board = BoardState.DefaultPositions;
            var game = new Game(board);
            var expectedStartSquares = board.AllPieces;

            game.Move("Na3");
            expectedStartSquares -= 2; // Move piece at idx = 2 (1^idx-1 = 2)
            Assert.That(game.CurrentState.PiecesOnStartSquares, Is.EqualTo(expectedStartSquares));

            game.Move("Na6");
            expectedStartSquares = game.CurrentState.PiecesOnStartSquares;

            game.Move("Rb1");
            expectedStartSquares -= 1; // Move piece at idx = 1 to a square already flipped (1^idx-1 = 1)
            Assert.That(game.CurrentState.PiecesOnStartSquares, Is.EqualTo(expectedStartSquares));

            //Ensure they don't flip back on when moving to original square
            game.Move("Nb8");
            game.Move("Ra1");
            Assert.That(game.CurrentState.PiecesOnStartSquares, Is.EqualTo(expectedStartSquares));
        }

        [TestCase("e4,e5,Bd3,Bd6,Nh3,Nh6,O-O", "g1", "f1", SquareContents.White)]
        [TestCase("e4,e5,Bd3,Bd6,Nh3,Nh6,a3,O-O", "g8", "f8", SquareContents.Black)]
        [TestCase("d4,d5,Be3,Be6,Na3,Na6,Qd2,Qd7,O-O-O", "c1", "d1", SquareContents.White)]
        [TestCase("d4,d5,Be3,Be6,Na3,Na6,Qd2,Qd7,h3,O-O-O", "c8", "d8", SquareContents.Black)]
        public void Move_UpdatesPiecesWhenCastling(string input, string kingSquare, string rookSquare, SquareContents color)
        {
            var king = MoveParser.ParseSquare(kingSquare);
            var rook = MoveParser.ParseSquare(rookSquare);

            var game = new Game();
            foreach (var move in input.Split(','))
                game.Move(move);

            var expectedKing = game.GetSquareContents(king.File, king.Rank);
            var expectedRook = game.GetSquareContents(rook.File, rook.Rank);

            Assert.That(expectedKing, Is.EqualTo(SquareContents.King | color));
            Assert.That(expectedRook, Is.EqualTo(SquareContents.Rook | color));
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
        [TestCase("4k3/8/8/8/8/4p3/8/4K3", "e2", ExpectedResult = AttackState.None)]
        public AttackState Move_DetectsAttackState(string position, string move)
        {
            var fen = new FenSerializer();
            var board = fen.Deserialize(position);
            var game = new Game(board, PieceColor.Black);

            var error = game.Move(move);
            Assert.That(error, Is.EqualTo(ErrorCondition.None));

            return game.AttackState;
        }

        [TestCase("8/P7/8/8/8/8/8/K6k", "a8=Q", ExpectedResult = AttackState.Check)]
        [TestCase("8/P7/8/8/8/8/7p/K5Bk", "a8=Q", ExpectedResult = AttackState.Check, Description = "Can capture to escape mate")]
        [TestCase("8/P7/8/8/8/8/7p/K5nk", "a8=Q", ExpectedResult = AttackState.Check, Description = "Can block to escape mate")]
        [TestCase("8/P7/8/8/8/8/7p/K5bk", "a8=Q", ExpectedResult = AttackState.Checkmate, Description = "Can't escape mate")]
        public AttackState Move_DetectsAttackState_ForPromotion(string position, string move)
        {
            var fen = new FenSerializer();
            var board = fen.Deserialize(position);
            var game = new Game(board, PieceColor.White);

            var error = game.Move(move);
            Assert.That(error, Is.EqualTo(ErrorCondition.None));

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

            for (var i = 0; i < moves.Length - 1; i++)
            {
                game.Move(moves[i]);
                Assert.That(game.AttackState, Is.Not.EqualTo(AttackState.DrawByRepetition), $"Unexpected repetition at ply {i}");
            }

            var lastMove = moves.Last();
            game.Move(lastMove);
            Assert.That(game.AttackState, Is.EqualTo(AttackState.DrawByRepetition));
        }

        [Test]
        public void Move_DetectsDrawByRepetition_ConsidersCastlingAbility()
        {
            var game = new Game();
            var movesToSetup = new[]
            {
                "e4", "e5",
                "Bc4", "Bc5",
                "Nh3", "Nh6"
            };

            foreach(var move in movesToSetup)
                game.Move(move);

            var boardStateToDuplicate = game.CurrentState.Board;

            game.Move("Rg1");
            game.Move("Rg8");
            game.Move("Rh1");
            game.Move("Rh8");

            var movesToReplicate = new[]
            {
                "Ng1", "Ng8",
                "Nh3", "Nh6",
            };

            foreach (var move in movesToReplicate)
                game.Move(move);

            var repetitionCount = game.CurrentState.PossibleRepeatedHistory.Count(o => BoardState.Equals(o.Item1, boardStateToDuplicate));
            Assert.That(repetitionCount, Is.EqualTo(3));
            Assert.That(game.AttackState, Is.EqualTo(AttackState.None));
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

        [Test]
        public void Undo_FailsOnInitialState()
        {
            var game= new Game();
            var didUndo = game.Undo();

            Assert.That(didUndo, Is.False);
        }

        [Test]
        public void Undo_WillRevertUntilInitialState()
        {
            var game = new Game();
            var moves = new[] { "e4", "e5", "d4", "d6" };

            for (var i = 0; i < moves.Length; i++)
                game.Move(moves[i]);

            for (var i = 0; i < moves.Length; i++)
            {
                var didUndo = game.Undo();
                Assume.That(didUndo, Is.True);
            }

            var canUndo = game.Undo();
            Assert.That(canUndo, Is.False);
        }

        [Test]
        public void Undo_WillSetExpectedNextTurn()
        {
            var game = new Game();
            var moves = new[] { "e4", "e5", "d4", "d6", "Na3" };
            var nextTurn = new[] { PieceColor.White, PieceColor.Black, PieceColor.White, PieceColor.Black, PieceColor.White };

            for (var i = 0; i < moves.Length; i++)
                game.Move(moves[i]);

            for (var i = 0; i < moves.Length; i++)
            {
                var didUndo = game.Undo();
                Assume.That(didUndo, Is.True);

                var currentTurn = game.GetTurn();
                Assert.That(currentTurn, Is.EqualTo(nextTurn[i]));
            }
        }

        [Test]
        public void Undo_WillRestoreCapturedPiece()
        {
            var game = new Game();
            var moves = new[] { "Nc3", "d5", "Nxd5" };

            for (var i = 0; i < moves.Length; i++)
                game.Move(moves[i]);

            var targetSquare = game.GetSquareContents('d', 5);
            Assume.That(targetSquare, Is.EqualTo(SquareContents.White | SquareContents.Knight));

            game.Undo();
            targetSquare = game.GetSquareContents('d', 5);
            Assert.That(targetSquare, Is.EqualTo(SquareContents.Black | SquareContents.Pawn));
        }

        [Test]
        public void PawnPromotion_WillDetectCheckmate()
        {
            const string fen = "rnb1kbnr/pppp1ppp/8/8/4q3/1P5P/P1PP1Kp1/RNBQ1BNR";
            const string move = "gxh1=N#";

            var board = new FenSerializer().Deserialize(fen);
            var game = new Game(board, PieceColor.Black);

            var result = game.Move(move);

            Assert.That(result, Is.EqualTo(ErrorCondition.None));
            Assert.That(game.AttackState, Is.EqualTo(AttackState.Checkmate));
        }
    }
}
