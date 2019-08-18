using ChessLibrary.Models;
using NUnit.Framework;
using System.Linq;

namespace ChessLibrary.Tests
{
    [TestFixture]
    public class MoveGeneratorTests
    {
        [TestCase("h5", SquareContents.White | SquareContents.Queen, "e2 e4", "h7 h5", "d1 h5")]
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

        [TestCase("h5", SquareContents.White | SquareContents.King, 5)]
        [TestCase("a2", SquareContents.White | SquareContents.King, 5)]
        public void GeneratesExpectedSquares_MatchesCount(string sq, SquareContents piece, int expectedSquares)
        {
            var board = BoardState.Empty;
            var square = MoveParser.ParseSquare(sq);
            ulong bitSquare = BitTranslator.TranslateToBit(square.File, square.Rank);

            ApplyToBoard(bitSquare, piece, board);
            var game = new Game(board);

            var validMoves = game.GetValidMoves(square.File, square.Rank);

            Assert.That(validMoves.Count, Is.EqualTo(expectedSquares));
        }

        [TestCase("h5", SquareContents.White | SquareContents.King, "h4,g4,g5,g6,h6")]
        [TestCase("a2", SquareContents.White | SquareContents.King, "a1,b1,b2,b3,a3")]
        public void GeneratesExpectedSquares_HasExact(string sq, SquareContents piece, string expectedSquares)
        {
            var board = BoardState.Empty;
            var square = MoveParser.ParseSquare(sq);
            ulong bitSquare = BitTranslator.TranslateToBit(square.File, square.Rank);
            var expected = expectedSquares.Split(',').Select(MoveParser.ParseSquare).ToArray();

            ApplyToBoard(bitSquare, piece, board);
            var game = new Game(board);

            var validMoves = game.GetValidMoves(square.File, square.Rank).ToArray();

            Assert.That(validMoves, Is.EquivalentTo(expected));
        }

        private static void ApplyToBoard(ulong bitSquare, SquareContents contents, BoardState board)
        {
            if (contents.HasFlag(SquareContents.White))
            {
                board.WhitePieces |= bitSquare;
            }
            else if (contents.HasFlag(SquareContents.Black))
            {
                board.BlackPieces |= bitSquare;
            }

            if (contents.HasFlag(SquareContents.Pawn))
            {
                board.Pawns |= bitSquare;
            }
            else if (contents.HasFlag(SquareContents.Knight))
            {
                board.Knights |= bitSquare;
            }
            else if (contents.HasFlag(SquareContents.Bishop))
            {
                board.Bishops |= bitSquare;
            }
            else if (contents.HasFlag(SquareContents.Rook))
            {
                board.Rooks |= bitSquare;
            }
            else if (contents.HasFlag(SquareContents.Queen))
            {
                board.Queens |= bitSquare;
            }
            else if (contents.HasFlag(SquareContents.King))
            {
                board.Kings |= bitSquare;
            }
        }
    }
}