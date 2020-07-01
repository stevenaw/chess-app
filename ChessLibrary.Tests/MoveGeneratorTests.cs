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
                var move = game.ParseMove(moveString);
                var result = game.Move(move);
                Assert.That(result, Is.EqualTo(ErrorCondition.None), "Failed for move: " + moveString);
            }

            var targetSquare = MoveParser.ParseSquare(focusSquare);
            var actualContents = game.GetSquareContents(targetSquare.File, targetSquare.Rank);
            Assert.That(actualContents, Is.EqualTo(expectedContents));
        }

        [Test]
        public void Queen_CanMoveFromEdge()
        {
            var startSquare = MoveParser.ParseSquare("h5");
            var endSquare = MoveParser.ParseSquare("e8");
            var endBit = BitTranslator.TranslateToBit(endSquare.File, endSquare.Rank);

            BoardState board = BoardState.Empty
                .SetPiece(startSquare, SquareContents.White | SquareContents.Queen)
                .SetPiece(endSquare, SquareContents.Black | SquareContents.King);
            GameState state = GameState.Initialize(board);

            var squares = MoveGenerator.GenerateStandardMoves(state, board.WhitePieces, 0);

            Assert.That(squares & endBit, Is.Not.EqualTo(0));
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
        [TestCase("d4,d5,Be3,Be6,Na3,Na6,Qd2,Qd7,a3", "e8", "c8")]
        public void GeneratesExpectedSquares_AllowsCastling_WhenSpacesOpen(string input,string kingSquare, string expectedCastlingSquare)
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

        [TestCase("e4,e5,Bd3,Bd6,Ne2,Nh6,f4,Qh4", "e1", "g1", Description = "Under attack")]
        public void GeneratesExpectedSquares_DisallowsCastling_WhenUnderAttack(string input, string kingSquare, string expectedCastlingSquare)
        {
            var king = MoveParser.ParseSquare(kingSquare);
            var expectedResult = MoveParser.ParseSquare(expectedCastlingSquare);
            IEnumerable<Square> validMoves;

            var game = new Game();
            var moves = input.Split(',');

            foreach (var move in moves.SkipLast(1))
                game.Move(move);

            game.Move(moves.Last());

            Assert.That(game.AttackState, Is.EqualTo(AttackState.Check));

            validMoves = game.GetValidMoves(king.File, king.Rank);
            Assert.That(validMoves, Does.Not.Contain(expectedResult));
        }

        [TestCase("e4,e5,Bd3,Bd6,Ne2,Nh6,f4,Qh4,g3,Qxf4", "e1", "g1", Description = "Passing through attack")]
        [TestCase("e4,e5,Bd3,Bd6,Ne2,Nh6,f4,Qh4,g3,Qxh2", "e1", "g1", Description = "Ending in attack")]
        public void GeneratesExpectedSquares_DisallowsCastling_WhenPassThroughAttack(string input, string kingSquare, string expectedCastlingSquare)
        {
            var king = MoveParser.ParseSquare(kingSquare);
            var expectedResult = MoveParser.ParseSquare(expectedCastlingSquare);
            IEnumerable<Square> validMoves;

            var game = new Game();
            var moves = input.Split(',');

            foreach (var move in moves.SkipLast(1))
                game.Move(move);

            game.Move(moves.Last());
            Assert.That(game.AttackState, Is.Not.EqualTo(AttackState.Check));

            validMoves = game.GetValidMoves(king.File, king.Rank);
            Assert.That(validMoves, Does.Not.Contain(expectedResult));
        }

        [TestCase("8/8/8/8/7B/8/8/K1k5", "h4", "e1")]
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
    }
}