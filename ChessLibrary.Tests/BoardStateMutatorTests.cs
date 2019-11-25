using ChessLibrary.Models;
using NUnit.Framework;

namespace ChessLibrary.Tests
{
    [TestFixture]
    public class BoardStateMutatorTests
    {
        [Test]
        public void MovePiece_ShouldSetNewPosition()
        {
            var oldState = BoardState.DefaultPositions;
            var from = 1UL << 9;
            var to = 1UL << 17;

            var newState = oldState.MovePiece(from, to);

            Assert.That(newState.Pawns & to, Is.Not.Zero);
            Assert.That(newState.WhitePieces & to, Is.Not.Zero);
        }

        [Test]
        public void MovePiece_ShouldClearOldPosition()
        {
            var oldState = BoardState.DefaultPositions;
            var from = 1UL << 9;
            var to = 1UL << 17;

            var newState = oldState.MovePiece(from, to);

            Assert.That(newState.Pawns & from, Is.Zero);
            Assert.That(newState.WhitePieces & from, Is.Zero);
        }

        [Test]
        public void MovePiece_ShouldClearPreviousStateOfNewPosition()
        {
            var oldState = BoardState.DefaultPositions;
            var from = 1UL << 9;
            var to = 1UL << 63;

            Assert.That(oldState.Rooks & to, Is.Not.Zero);
            Assert.That(oldState.BlackPieces & to, Is.Not.Zero);
            Assert.That(oldState.Pawns & from, Is.Not.Zero);
            Assert.That(oldState.WhitePieces & from, Is.Not.Zero);

            var newState = oldState.MovePiece(from, to);

            Assert.That(newState.Rooks & to, Is.Zero);
            Assert.That(newState.BlackPieces & to, Is.Zero);
            Assert.That(newState.Pawns & to, Is.Not.Zero);
            Assert.That(newState.WhitePieces & to, Is.Not.Zero);
        }
    }
}
