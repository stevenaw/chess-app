using ChessLibrary.Models;
using NUnit.Framework;
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
            var moves = moveStrings.Select(game.ParseMove).ToArray();

            foreach(var move in moves)
            {
                var result = game.Move(move.StartFile, move.StartRank, move.EndFile, move.EndRank);
                Assert.That(result, Is.EqualTo(ErrorConditions.None));
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

            var squares = MoveGenerator.GenerateStandardMoves(board, board.WhitePieces, 0);

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
    }
}