using ChessLibrary.Models;
using ChessLibrary.Serialization;
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
    }
}
