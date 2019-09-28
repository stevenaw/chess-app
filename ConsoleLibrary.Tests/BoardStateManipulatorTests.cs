using ChessLibrary.Models;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace ChessLibrary.Tests
{
    [TestFixture]
    public class BoardStateManipulatorTests
    {
        [Test]
        public void MovePiece_ShouldSetNewPosition()
        {
            var state = BoardState.DefaultPositions;
            var from = 1UL << 9;
            var to = 1UL << 17;

            BoardStateManipulator.MovePiece(state, from, to);

            Assert.That(state.Pawns & to, Is.Not.Zero);
            Assert.That(state.WhitePieces & to, Is.Not.Zero);
        }

        [Test]
        public void MovePiece_ShouldClearOldPosition()
        {
            var state = BoardState.DefaultPositions;
            var from = 1UL << 9;
            var to = 1UL << 17;

            BoardStateManipulator.MovePiece(state, from, to);

            Assert.That(state.Pawns & from, Is.Zero);
            Assert.That(state.WhitePieces & from, Is.Zero);
        }

        [Test]
        public void MovePiece_ShouldClearPreviousStateOfNewPosition()
        {
            var state = BoardState.DefaultPositions;
            var from = 1UL << 9;
            var to = 1UL << 63;

            Assert.That(state.Rooks & to, Is.Not.Zero);
            Assert.That(state.BlackPieces & to, Is.Not.Zero);
            Assert.That(state.Pawns & from, Is.Not.Zero);
            Assert.That(state.WhitePieces & from, Is.Not.Zero);

            BoardStateManipulator.MovePiece(state, from, to);

            Assert.That(state.Rooks & to, Is.Zero);
            Assert.That(state.BlackPieces & to, Is.Zero);
            Assert.That(state.Pawns & to, Is.Not.Zero);
            Assert.That(state.WhitePieces & to, Is.Not.Zero);
        }
    }
}
