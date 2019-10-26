using ChessLibrary.Models;
using ChessLibrary.Serialization;
using NUnit.Framework;
using System;

namespace ChessLibrary.Tests
{
    [TestFixture]
    public class FenSerializerTests
    {
        [Test]
        public void Deserialize_ShouldSetBoardState_WhenValidStringGiven()
        {
            const string FenString = "pQ4N1/3k3R/1r4n1/KbBbBppP/8/8/q7/7n";
            FenSerializer ser = new FenSerializer();
            BoardState expected = new BoardState();
            BoardState actual;

            // Setup
            BoardStateManipulator.SetPiece(expected, "a8", SquareContents.Black | SquareContents.Pawn);
            BoardStateManipulator.SetPiece(expected, "b8", SquareContents.White | SquareContents.Queen);
            BoardStateManipulator.SetPiece(expected, "g8", SquareContents.White | SquareContents.Knight);

            BoardStateManipulator.SetPiece(expected, "d7", SquareContents.Black | SquareContents.King);
            BoardStateManipulator.SetPiece(expected, "h7", SquareContents.White | SquareContents.Rook);

            BoardStateManipulator.SetPiece(expected, "b6", SquareContents.Black | SquareContents.King);
            BoardStateManipulator.SetPiece(expected, "g6", SquareContents.Black | SquareContents.Knight);

            BoardStateManipulator.SetPiece(expected, "a5", SquareContents.White | SquareContents.King);
            BoardStateManipulator.SetPiece(expected, "b5", SquareContents.Black | SquareContents.Bishop);
            BoardStateManipulator.SetPiece(expected, "c5", SquareContents.White | SquareContents.Bishop);
            BoardStateManipulator.SetPiece(expected, "d5", SquareContents.Black | SquareContents.Bishop);
            BoardStateManipulator.SetPiece(expected, "e5", SquareContents.White | SquareContents.Bishop);
            BoardStateManipulator.SetPiece(expected, "f5", SquareContents.Black | SquareContents.Pawn);
            BoardStateManipulator.SetPiece(expected, "g5", SquareContents.Black | SquareContents.Pawn);
            BoardStateManipulator.SetPiece(expected, "h5", SquareContents.White | SquareContents.Pawn);

            BoardStateManipulator.SetPiece(expected, "a2", SquareContents.Black | SquareContents.Queen);
            BoardStateManipulator.SetPiece(expected, "h1", SquareContents.Black | SquareContents.Knight);

            // Act
            actual = ser.Deserialize(FenString);

            Assert.AreEqual(expected.AllPieces, actual.AllPieces);
            Assert.AreEqual(expected.WhitePieces, actual.WhitePieces);
            Assert.AreEqual(expected.BlackPieces, actual.BlackPieces);
        }

        [Test]
        public void Deserialize_ShouldThrowException_WhenEmpty()
        {
            const string FenString = "";

            var serializer = new FenSerializer();
            var exception = Assert.Throws<ArgumentNullException>(() => serializer.Deserialize(FenString));
        }

        [Test]
        public void Deserialize_ShouldThrowException_WhenTooLong()
        {
            var FenString = new string('0', 80);

            var serializer = new FenSerializer();
            var exception = Assert.Throws<ArgumentException>(() => serializer.Deserialize(FenString));
        }

        [Test]
        public void Deserialize_ShouldThrowException_WhenTooFewRows()
        {
            const string FenString = "8/8/8/8/8/8/q7";
            const string ExpectedMessagee = "Not enough rows specified!";

            var serializer = new FenSerializer();
            var exception = Assert.Throws<InvalidOperationException>(() => serializer.Deserialize(FenString));

            Assert.AreEqual(ExpectedMessagee, exception.Message);
        }

        [Test]
        public void Deserialize_ShouldThrowException_WhenTooManyRows()
        {
            const string FenString = "8/8/8/8/8/8/q7/8/8";
            const string ExpectedMessagee = "Too many rows!";

            var serializer = new FenSerializer();
            var exception = Assert.Throws<InvalidOperationException>(() => serializer.Deserialize(FenString));

            Assert.AreEqual(ExpectedMessagee, exception.Message);
        }

        [Test]
        public void Deserialize_ShouldThrowException_WhenEndsInDelimeterAndTooManyRows()
        {
            const string FenString = "8/8/8/8/8/8/q7/8/";
            const string ExpectedMessagee = "Too many rows!";

            var serializer = new FenSerializer();
            var exception = Assert.Throws<InvalidOperationException>(() => serializer.Deserialize(FenString));

            Assert.AreEqual(ExpectedMessagee, exception.Message);
        }

        [Test]
        public void Deserialize_ShouldThrowException_WhenEndsInDelimeterAndTooFewRows()
        {
            const string FenString = "8/8/8/8/8/8/q7/";
            const string ExpectedMessagee = "Not enough columns specified!";

            var serializer = new FenSerializer();
            var exception = Assert.Throws<InvalidOperationException>(() => serializer.Deserialize(FenString));

            Assert.AreEqual(ExpectedMessagee, exception.Message);
        }

        [Test]
        public void Deserialize_ShouldThrowException_WhenNotEnoughFilledSquares()
        {
            const string FenString = "8/8/8/8/8/8/bbbbbbb/8";
            const string ExpectedMessagee = "Unanticipated new row, columns to fill!";

            var serializer = new FenSerializer();
            var exception = Assert.Throws<InvalidOperationException>(() => serializer.Deserialize(FenString));

            Assert.AreEqual(ExpectedMessagee, exception.Message);
        }

        [Test]
        public void Deserialize_ShouldThrowException_WhenNotEnoughEmptySquares()
        {
            const string FenString = "8/8/8/8/8/8/7/8";
            const string ExpectedMessagee = "Unanticipated new row, columns to fill!";

            var serializer = new FenSerializer();
            var exception = Assert.Throws<InvalidOperationException>(() => serializer.Deserialize(FenString));

            Assert.AreEqual(ExpectedMessagee, exception.Message);
        }

        [Test]
        public void Deserialize_ShouldThrowException_WhenTooManyEmptySquares()
        {
            const string FenString = "8/8/8/8/8/8/9/8";
            const string ExpectedMessagee = "Blank spot exceeds column count!";

            var serializer = new FenSerializer();
            var exception = Assert.Throws<InvalidOperationException>(() => serializer.Deserialize(FenString));

            Assert.AreEqual(ExpectedMessagee, exception.Message);
        }

        [Test]
        public void Deserialize_ShouldThrowException_WhenTooManyFilledSquares()
        {
            const string FenString = "8/8/8/8/8/8/bbbbbbbbb/8";
            const string ExpectedMessagee = "Too many pieces specified in row!";

            var serializer = new FenSerializer();
            var exception = Assert.Throws<InvalidOperationException>(() => serializer.Deserialize(FenString));

            Assert.AreEqual(ExpectedMessagee, exception.Message);
        }

        [Test]
        public void Deserialize_ShouldThrowException_WhenInvalidEmptyToken()
        {
            const string FenString = "8/8/8/8/8/8/bb0bbbbb/8";
            const string ExpectedMessagee = "Can not have a 0 in a FEN diagram!";

            var serializer = new FenSerializer();
            var exception = Assert.Throws<InvalidOperationException>(() => serializer.Deserialize(FenString));

            Assert.AreEqual(ExpectedMessagee, exception.Message);
        }

        [Test]
        public void Deserialize_ShouldThrowException_WhenConsecutiveEmptyToken()
        {
            const string FenString = "8/8/8/8/8/8/bb11bbbb/8";
            const string ExpectedMessagee = "Can not specify two consecutive blank spots!";

            var serializer = new FenSerializer();
            var exception = Assert.Throws<InvalidOperationException>(() => serializer.Deserialize(FenString));

            Assert.AreEqual(ExpectedMessagee, exception.Message);
        }

        [Test]
        public void Deserialize_ShouldThrowException_WhenInvalidFilledSquare()
        {
            const string FenString = "8/8/8/8/8/8/bb1cbbbb/8";
            const string ExpectedMessagee = "Unsupported piece notation: c";

            var serializer = new FenSerializer();
            var exception = Assert.Throws<InvalidOperationException>(() => serializer.Deserialize(FenString));

            Assert.AreEqual(ExpectedMessagee, exception.Message);
        }

        [Test]
        public void Serialize_ShouldSerializeCorrectPosition()
        {
            const string expectedValue = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR";

            var board = BoardState.DefaultPositions;
            var serializer = new FenSerializer();

            var actualValue = serializer.Serialize(board);

            Assert.AreEqual(expectedValue, actualValue);
        }
    }
}
