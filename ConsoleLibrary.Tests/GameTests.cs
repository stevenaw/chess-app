using ChessLibrary.Models;
using NUnit.Framework;
using System;
using System.Collections.Generic;
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
            var board = BoardState.Empty;
            var endSq = MoveParser.ParseSquare(expectedEnd);
            var startSq = MoveParser.ParseSquare(expectedStart);
            var pieceColor = color == PieceColor.White ? SquareContents.White : SquareContents.Black;

            BoardStateManipulator.SetPiece(board, startSq, SquareContents.Pawn | pieceColor);

            var game = new Game(board, color);
            var result = game.Move(input);
            var expectedSquare = game.GetSquareContents(endSq.File, endSq.Rank);

            Assert.That(result, Is.EqualTo(ErrorConditions.None));
            Assert.That(expectedSquare, Is.EqualTo(piece));
        }
    }
}
