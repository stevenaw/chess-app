using ChessLibrary.Models;
using ChessLibrary.Serialization;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace ChessLibrary.Tests
{
    [TestFixture]
    public class MoveGeneratorTests
    {
        [TestCase("h5", SquareContents.White | SquareContents.Queen, "e2-e4", "h7-h5", "d1-h5")]
        public void PlacesPieceOnIntendedSquare(string focusSquare, SquareContents expectedContents, params string[] moveStrings)
        {
            var game = new Game();

            foreach(var moveString in moveStrings)
            {
                var result = game.Move(moveString);
                Assert.That(result, Is.EqualTo(ErrorCondition.None), "Failed for move: " + moveString);
            }

            var targetSquare = MoveParser.ParseSquare(focusSquare);
            var actualContents = game.GetSquareContents(targetSquare.File, targetSquare.Rank);
            Assert.That(actualContents, Is.EqualTo(expectedContents));
        }

        [TestCase("8/8/8/7Q/8/8/8/K1k5", "h5", "d1", Description = "Queen")]
        [TestCase("8/8/8/8/7B/8/8/K1k5", "h4", "e1", Description = "Bishop")]
        public void GetValidMoves_DetectsWhenOnEdge(string position, string targetSq, string expectedSq)
        {
            var fen = new FenSerializer();
            var board = fen.Deserialize(position);

            var target = MoveParser.ParseSquare(targetSq);
            var targetMask = BitTranslator.TranslateToBit(target.File, target.Rank);

            var validMovesMask = MoveGenerator.GetBishopMovements(targetMask, board);
            var validMoves = BitTranslator.TranslateToSquares(validMovesMask);

            var expected = MoveParser.ParseSquare(expectedSq);
            Assert.That(validMoves, Does.Contain(expected));
        }

        [TestCase("h5", SquareContents.White | SquareContents.King, 5)]
        [TestCase("a2", SquareContents.White | SquareContents.King, 5)]
        public void GeneratesExpectedSquares_MatchesCount(string sq, SquareContents piece, int expectedSquares)
        {
            var square = MoveParser.ParseSquare(sq);
            ulong bitSquare = BitTranslator.TranslateToBit(square.File, square.Rank);
            var board = BoardState.Empty.SetPiece(bitSquare, piece);
            var game = new Game(board);

            var validMoves = game.GetValidMoves(square.File, square.Rank);

            Assert.That(validMoves.Count, Is.EqualTo(expectedSquares));
        }

        [TestCase("h5", SquareContents.White | SquareContents.King, "h4,g4,g5,g6,h6")]
        [TestCase("a2", SquareContents.White | SquareContents.King, "a1,b1,b2,b3,a3")]
        public void GeneratesExpectedSquares_HasExact(string sq, SquareContents piece, string expectedSquares)
        {
            var square = MoveParser.ParseSquare(sq);
            ulong bitSquare = BitTranslator.TranslateToBit(square.File, square.Rank);
            var board = BoardState.Empty.SetPiece(bitSquare, piece);

            var expected = expectedSquares.Split(',').Select(MoveParser.ParseSquare).ToArray();

            var game = new Game(board);

            var validMoves = game.GetValidMoves(square.File, square.Rank).ToArray();

            Assert.That(validMoves, Is.EquivalentTo(expected));
        }

        [TestCase("b1", SquareContents.White | SquareContents.Bishop, "a2,c2")]
        public void GeneratesExpectedSquares_ContainsExpected(string sq, SquareContents piece, string expectedSquares)
        {
            var square = MoveParser.ParseSquare(sq);
            ulong bitSquare = BitTranslator.TranslateToBit(square.File, square.Rank);
            var board = BoardState.Empty.SetPiece(bitSquare, piece);
            var expected = expectedSquares.Split(',').Select(MoveParser.ParseSquare).ToArray();

            var game = new Game(board);

            var validMoves = game.GetValidMoves(square.File, square.Rank).ToArray();

            Assert.That(validMoves, Contains.Item(expected[0]));
            Assert.That(validMoves, Contains.Item(expected[1]));
        }

        [TestCase("b1", SquareContents.Black | SquareContents.Bishop, "a1")]
        public void GeneratesExpectedSquares_OmitsUnexpected(string sq, SquareContents piece, string expectedSquares)
        {
            var expected = expectedSquares.Split(',').Select(MoveParser.ParseSquare).ToArray();

            var square = MoveParser.ParseSquare(sq);
            ulong bitSquare = BitTranslator.TranslateToBit(square.File, square.Rank);
            var board = BoardState.Empty.SetPiece(bitSquare, piece);

            var game = new Game(board);

            var validMoves = game.GetValidMoves(square.File, square.Rank).ToArray();

            Assert.That(validMoves, Does.Not.Contain(expected[0]));
        }

        [TestCase("e4,e5,Bd3,Bd6,Nh3,Nh6", "e1", "g1")]
        [TestCase("e4,e5,Bd3,Bd6,Nh3,Nh6,a3", "e8", "g8")]
        [TestCase("d4,d5,Be3,Be6,Na3,Na6,Qd2,Qd7", "e1", "c1")]
        [TestCase("d4,c5,Be3,Qa5,Qd2,Na6,Na3,Qxa2", "e1", "c1", Description = "a1+b1 under attack, but others are clear")]
        [TestCase("d4,d5,Be3,Be6,Na3,Na6,Qd2,Qd7,a3", "e8", "c8")]
        public void King_AllowsCastling_WhenSpacesOpen(string input,string kingSquare, string expectedCastlingSquare)
        {
            var king = MoveParser.ParseSquare(kingSquare);
            var expectedResult = MoveParser.ParseSquare(expectedCastlingSquare);
            IEnumerable<Square> validMoves;

            var game = new Game();
            var moves = input.Split(',');

            foreach (var move in moves.SkipLast(2))
            {
                game.Move(move);

                validMoves = game.GetValidMoves(king.File, king.Rank);
                Assert.That(validMoves, Does.Not.Contain(expectedResult), $"Unexpected castling square after move {move}");
            }

            foreach (var move in moves.TakeLast(2))
                game.Move(move);

            validMoves = game.GetValidMoves(king.File, king.Rank);
            Assert.That(validMoves, Does.Contain(expectedResult));
        }

        [TestCase("e4,e5,Bd3,Bd6,Ne2,Nh6,f4,Qh4", "e1", "g1", Description = "Under attack - kingside")]
        [TestCase("d4,c5,Be3,Nh6,Na3,Na6,Qd3,Qa5", "e1", "c1", Description = "Under attack - queenside")]
        public void King_DisallowsCastling_WhenUnderAttack(string input, string kingSquare, string expectedCastlingSquare)
        {
            var king = MoveParser.ParseSquare(kingSquare);
            var expectedResult = MoveParser.ParseSquare(expectedCastlingSquare);

            var game = new Game();
            var moves = input.Split(',');

            foreach (var move in moves.SkipLast(1))
                game.Move(move);

            game.Move(moves.Last());
            Assert.That(game.AttackState, Is.EqualTo(AttackState.Check));

            var validMoves = game.GetValidMoves(king.File, king.Rank);
            Assert.That(validMoves, Does.Not.Contain(expectedResult));
        }

        [TestCase("e4,e5,Bd3,Bd6,Ne2,Nh6,f4,Qh4,g3,Qxf4", "e1", "g1", Description = "Passing through attack")]
        [TestCase("e4,e5,Bd3,Bd6,Ne2,Nh6,f4,Qh4,g3,Qxh2", "e1", "g1", Description = "Ending in attack")]
        [TestCase("g4,e5,g3,Bd3,gxh2,Nh3,Nh6", "e1", "g1", Description = "Ending in attack (from pawn)")]
        [TestCase("e4,e5,Bd3,Bd6,Nh3,Nh6,Rg1,Na6,Rh1,Nb8", "e1", "g1", Description = "Rook moved already")]
        [TestCase("e4,e5,Bd3,Bd6,Nh3,Nh6,Kf1,Na6,Ke1,Nb8", "e1", "g1", Description = "King moved already")]
        public void King_DisallowsCastling_WhenInvalidState(string input, string kingSquare, string expectedCastlingSquare)
        {
            var king = MoveParser.ParseSquare(kingSquare);
            var expectedResult = MoveParser.ParseSquare(expectedCastlingSquare);

            var game = new Game();
            var moves = input.Split(',');

            foreach (var move in moves.SkipLast(1))
                game.Move(move);

            game.Move(moves.Last());
            Assert.That(game.AttackState, Is.Not.EqualTo(AttackState.Check));

            var validMoves = game.GetValidMoves(king.File, king.Rank);
            Assert.That(validMoves, Does.Not.Contain(expectedResult));
        }

        [Test]
        public void EnPassant_IsAllowed_ImmediatelyAfterPush()
        {
            var game = new Game();
            var moveStrings = new[]
            {
                "e4", "Na6", "e5", "d5"
            };

            foreach (var moveString in moveStrings)
            {
                var result = game.Move(moveString);
                Assert.That(result, Is.EqualTo(ErrorCondition.None));
            }

            var validMoves = game.GetValidMoves('e', 5);
            Assert.That(validMoves, Does.Contain(new Square('d', 6)));
        }


        [Test]
        public void EnPassant_IsDisallowed_WhenNotImmediatelyAfterPush()
        {
            var game = new Game();
            var moveStrings = new[]
            {
                "e4", "Na6", "e5", "d5", // Setup
                "Na3", "Nb8", // Wait a bit
            };

            for (var i = 0; i < moveStrings.Length; i++)
            {
                var result = game.Move(moveStrings[i]);
                Assert.That(result, Is.EqualTo(ErrorCondition.None));
            }

            var validMoves = game.GetValidMoves('e', 5);
            Assert.That(validMoves, Does.Not.Contain(new Square('d', 6)));
        }

        [Test]
        public void EnPassant_IsDisallowed_WhenNotPushed()
        {
            var game = new Game();
            var moveStrings = new[]
            {
                "e4", "Na6", "e5", "d6", "Na3", "d5", "exd6"
            };

            for (var i = 0; i < moveStrings.Length - 1; i++)
            {
                var result = game.Move(moveStrings[i]);
                Assert.That(result, Is.EqualTo(ErrorCondition.None));
            }

            var validMoves = game.GetValidMoves('e', 5);
            Assert.That(validMoves, Does.Not.Contain(new Square('d', 6)));
        }

        [TestCase("8/8/8/7k/8/8/7P/7K", "Kh4")]
        [TestCase("8/8/8/8/7k/8/7P/7K", "Kh3")]
        public void King_CanMoveIntoPawnMovementPath(string fen, string move)
        {
            var serializer = new FenSerializer();
            var board = serializer.Deserialize(fen);
            var game = new Game(board, PieceColor.Black);

            var result = game.Move(move);

            Assert.That(result, Is.EqualTo(ErrorCondition.None));
        }

        [TestCase("8/8/8/8/7k/8/7P/7K", "Kg3")]
        public void King_UnableMoveIntoPawnMovementPath(string fen, string move)
        {
            var serializer = new FenSerializer();
            var board = serializer.Deserialize(fen);
            var game = new Game(board, PieceColor.Black);

            var result = game.Move(move);

            Assert.That(result, Is.EqualTo(ErrorCondition.InvalidMovement));
        }
    }
}