using ChessLibrary.Models;
using ChessLibrary.Serialization;
using NUnit.Framework;

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
            BoardState actual;
            BoardState expected = BoardState.Empty
                    .SetPiece(MoveParser.ParseSquare("a8"), SquareContents.Black | SquareContents.Pawn)
                    .SetPiece(MoveParser.ParseSquare("b8"), SquareContents.White | SquareContents.Queen)
                    .SetPiece(MoveParser.ParseSquare("g8"), SquareContents.White | SquareContents.Knight)

                    .SetPiece(MoveParser.ParseSquare("d7"), SquareContents.Black | SquareContents.King)
                    .SetPiece(MoveParser.ParseSquare("h7"), SquareContents.White | SquareContents.Rook)

                    .SetPiece(MoveParser.ParseSquare("b6"), SquareContents.Black | SquareContents.King)
                    .SetPiece(MoveParser.ParseSquare("g6"), SquareContents.Black | SquareContents.Knight)

                    .SetPiece(MoveParser.ParseSquare("a5"), SquareContents.White | SquareContents.King)
                    .SetPiece(MoveParser.ParseSquare("b5"), SquareContents.Black | SquareContents.Bishop)
                    .SetPiece(MoveParser.ParseSquare("c5"), SquareContents.White | SquareContents.Bishop)
                    .SetPiece(MoveParser.ParseSquare("d5"), SquareContents.Black | SquareContents.Bishop)
                    .SetPiece(MoveParser.ParseSquare("e5"), SquareContents.White | SquareContents.Bishop)
                    .SetPiece(MoveParser.ParseSquare("f5"), SquareContents.Black | SquareContents.Pawn)
                    .SetPiece(MoveParser.ParseSquare("g5"), SquareContents.Black | SquareContents.Pawn)
                    .SetPiece(MoveParser.ParseSquare("h5"), SquareContents.White | SquareContents.Pawn)

                    .SetPiece(MoveParser.ParseSquare("a2"), SquareContents.Black | SquareContents.Queen)
                    .SetPiece(MoveParser.ParseSquare("h1"), SquareContents.Black | SquareContents.Knight);

            // Act
            actual = ser.Deserialize(FenString);

            Assert.That(expected.AllPieces, Is.EqualTo(actual.AllPieces));
            Assert.That(expected.WhitePieces, Is.EqualTo(actual.WhitePieces));
            Assert.That(expected.BlackPieces, Is.EqualTo(actual.BlackPieces));
        }

        [Test]
        public void Deserialize_ShouldThrowException_WhenEmpty()
        {
            var serializer = new FenSerializer();

            Assert.That(() => serializer.Deserialize(string.Empty), Throws.ArgumentNullException);
        }

        [Test]
        public void Deserialize_ShouldThrowException_WhenTooLong()
        {
            var fenString = new string('0', 80);

            var serializer = new FenSerializer();
            Assert.That(() => serializer.Deserialize(fenString), Throws.ArgumentException);
        }

        [TestCase("8/8/8/8/8/8/q7", "Not enough rows specified!")]
        [TestCase("8/8/8/8/8/8/q7/8/8", "Too many rows!")]
        [TestCase("8/8/8/8/8/8/q7/8/", "Too many rows!")]
        [TestCase("8/8/8/8/8/8/q7/", "Not enough columns specified!")]
        [TestCase("8/8/8/8/8/8/bbbbbbb/8", "Unanticipated new row, columns to fill!")]
        [TestCase("8/8/8/8/8/8/7/8", "Unanticipated new row, columns to fill!")]
        [TestCase("8/8/8/8/8/8/9/8", "Blank spot exceeds column count!")]
        [TestCase("8/8/8/8/8/8/bbbbbbbbb/8", "Too many pieces specified in row!")]
        [TestCase("8/8/8/8/8/8/bb0bbbbb/8", "Can not have a 0 in a FEN diagram!")]
        [TestCase("8/8/8/8/8/8/bb11bbbb/8", "Can not specify two consecutive blank spots!")]
        [TestCase("8/8/8/8/8/8/bb1cbbbb/8", "Unsupported piece notation: c")]
        public void Deserialize_ShouldThrowException_WhenInvalid(string fenString, string expectedMessage)
        {
            var serializer = new FenSerializer();

            Assert.That(() => serializer.Deserialize(fenString), Throws.InvalidOperationException.With.Message.EqualTo(expectedMessage));
        }

        [Test]
        public void Serialize_ShouldSerializeCorrectPosition()
        {
            var defaultFen = FenSerializer.DefaultValue;
            var board = BoardState.DefaultPositions;

            var serializer = new FenSerializer();

            var actualValue = serializer.Serialize(board);

            Assert.That(actualValue, Is.EqualTo(defaultFen));
        }
    }
}
