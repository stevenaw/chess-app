using ChessLibrary.Models;
using NUnit.Framework;

namespace ChessLibrary.Tests
{
    [TestFixture]
    public class MoveParserTests
    {
        [TestCase("0-0", PieceColor.White, "e1", "g1")]
        [TestCase("0-0-0", PieceColor.White, "e1", "c1")]
        [TestCase("Kd1", PieceColor.White, "e1", "d1")]
        [TestCase("Nc3", PieceColor.White, "b1", "c3")]

        [TestCase("0-0", PieceColor.Black, "e8", "g8")]
        [TestCase("0-0-0", PieceColor.Black, "e8", "c8")]
        [TestCase("Kd8", PieceColor.Black, "e8", "d8")]
        [TestCase("Nh6", PieceColor.Black, "g8", "h6")]
        public void TryParseMove_GeneratesProperSquare(string input, PieceColor color, string expectedStart, string expectedEnd)
        {
            var board = BoardState.DefaultPositions;
            var startSq = MoveParser.ParseSquare(expectedStart);
            var endSq = MoveParser.ParseSquare(expectedEnd);
            var activePieces = color == PieceColor.White ? board.WhitePieces : board.BlackPieces;
            Move move;

            var success = MoveParser.TryParseMove(input, board, activePieces, out move);

            Assert.That(success, Is.True);

            Assert.That(move.StartRank, Is.EqualTo(startSq.Rank));
            Assert.That(move.StartFile, Is.EqualTo(startSq.File));
            Assert.That(move.EndRank, Is.EqualTo(endSq.Rank));
            Assert.That(move.EndFile, Is.EqualTo(endSq.File));
        }

        [TestCase("a2 a4", SquareContents.Pawn | SquareContents.White, "a2", "a4")]
        [TestCase("a2-a4", SquareContents.Pawn | SquareContents.White, "a2", "a4")]
        [TestCase("a2xa4", SquareContents.Pawn | SquareContents.White, "a2", "a4")]
        [TestCase("a3", SquareContents.Pawn | SquareContents.White, "a2", "a3")]
        [TestCase("a4", SquareContents.Pawn | SquareContents.White, "a2", "a4")]
        [TestCase("d5", SquareContents.Pawn | SquareContents.Black, "d7", "d5")]
        [TestCase("d6", SquareContents.Pawn | SquareContents.Black, "d7", "d6")]

        [TestCase("Kb1 c2", SquareContents.King | SquareContents.White, "b1", "c2")]
        [TestCase("Kb1-c2", SquareContents.King | SquareContents.White, "b1", "c2")]
        [TestCase("Kb1xc2", SquareContents.King | SquareContents.White, "b1", "c2")]
        [TestCase("Kc2", SquareContents.King | SquareContents.White, "b1", "c2")]

        [TestCase("Qb1 d3", SquareContents.Queen | SquareContents.White, "b1", "d3")]
        [TestCase("Qb1-d3", SquareContents.Queen | SquareContents.White, "b1", "d3")]
        [TestCase("Qb1xd3", SquareContents.Queen | SquareContents.White, "b1", "d3")]
        [TestCase("Qd3", SquareContents.Queen | SquareContents.White, "b1", "d3")]

        [TestCase("Rb1 b4", SquareContents.Rook | SquareContents.White, "b1", "b4")]
        [TestCase("Rb1-b4", SquareContents.Rook | SquareContents.White, "b1", "b4")]
        [TestCase("Rb1xb4", SquareContents.Rook | SquareContents.White, "b1", "b4")]
        [TestCase("Rb4", SquareContents.Rook | SquareContents.White, "b1", "b4")]

        [TestCase("Bb1 d3", SquareContents.Bishop | SquareContents.White, "b1", "d3")]
        [TestCase("Bb1-d3", SquareContents.Bishop | SquareContents.White, "b1", "d3")]
        [TestCase("Bb1xd3", SquareContents.Bishop | SquareContents.White, "b1", "d3")]
        [TestCase("Bd3", SquareContents.Bishop | SquareContents.White, "b1", "d3")]

        [TestCase("Nb1 c3", SquareContents.Knight | SquareContents.White, "b1", "c3")]
        [TestCase("Nb1-c3", SquareContents.Knight | SquareContents.White, "b1", "c3")]
        [TestCase("Nb1xc3", SquareContents.Knight | SquareContents.White, "b1", "c3")]
        [TestCase("Nc3", SquareContents.Knight | SquareContents.White, "b1", "c3")]
        public void TryParseMove_GeneratesProperSquare_RegardlessOfSide(string input, SquareContents piece, string expectedStart, string expectedEnd)
        {
            var board = BoardState.Empty;
            var endSq = MoveParser.ParseSquare(expectedEnd);
            var startSq = MoveParser.ParseSquare(expectedStart);
            Move move;

            BoardStateManipulator.SetPiece(board, startSq, piece);
            var pieceMask = (piece & SquareContents.White) != 0 ? board.WhitePieces : board.BlackPieces;
            
            var success = MoveParser.TryParseMove(input, board, pieceMask, out move);

            Assert.That(success, Is.True);

            Assert.That(move.StartRank, Is.EqualTo(startSq.Rank));
            Assert.That(move.StartFile, Is.EqualTo(startSq.File));
            Assert.That(move.EndRank, Is.EqualTo(endSq.Rank));
            Assert.That(move.EndFile, Is.EqualTo(endSq.File));
        }
    }
}
