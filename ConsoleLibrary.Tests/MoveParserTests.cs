using ChessLibrary.Models;
using ChessLibrary.Serialization;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ChessLibrary.Tests
{
    [TestFixture]
    public class MoveParserTests
    {
        private static IEnumerable<TestCaseData> MoveAnnotationTestCases
        {
            get
            {
                var annotations = (MoveAnnotation[])Enum.GetValues(typeof(MoveAnnotation));
                var map = annotations.ToDictionary(
                    o => o,
                    o => MoveDescriptionHelper.GetAnnotationString(o)
                );

                var data = new[]
                {
                    ("0-0", "e1", "g1"),
                    ("0-0-0", "e1", "c1"),
                    ("Kd1", "e1", "d1"),
                    ("a4", "a2", "a4")
                };

                foreach(var datum in data)
                {
                    foreach(var kvp in map)
                    {
                        yield return new TestCaseData(
                            datum.Item1 + kvp.Value,
                            datum.Item2,
                            datum.Item3,
                            kvp.Key
                        );
                    }
                }
            }
        }
        private static IEnumerable<TestCaseData> AttackStateTestCases
        {
            get
            {
                var annotations = (AttackState[])Enum.GetValues(typeof(AttackState));
                var map = annotations.Where(a => a != AttackState.Stalemate).ToDictionary(
                    o => o,
                    o => MoveDescriptionHelper.GetAttackString(o)
                );

                var data = new[]
                {
                    ("0-0", "e1", "g1"),
                    ("0-0-0", "e1", "c1"),
                    ("Kd1", "e1", "d1"),
                    ("a4", "a2", "a4")
                };

                foreach (var datum in data)
                {
                    foreach (var kvp in map)
                    {
                        yield return new TestCaseData(
                            datum.Item1 + kvp.Value,
                            datum.Item2,
                            datum.Item3,
                            kvp.Key
                        );
                    }
                }
            }
        }

        [TestCaseSource(nameof(MoveAnnotationTestCases))]
        public void TryParseMove_ParsesAnnotation(string input, string expectedStart, string expectedEnd, MoveAnnotation expectedState)
        {
            var board = BoardState.DefaultPositions;
            var startSq = MoveParser.ParseSquare(expectedStart);
            var endSq = MoveParser.ParseSquare(expectedEnd);
            Move move;

            var success = MoveParser.TryParseMove(input, board, board.WhitePieces, out move);

            Assert.That(success, Is.True);

            Assert.That(move.StartRank, Is.EqualTo(startSq.Rank));
            Assert.That(move.StartFile, Is.EqualTo(startSq.File));
            Assert.That(move.EndRank, Is.EqualTo(endSq.Rank));
            Assert.That(move.EndFile, Is.EqualTo(endSq.File));
            Assert.That(move.Annotation, Is.EqualTo(expectedState));
        }

        [TestCaseSource(nameof(AttackStateTestCases))]
        public void TryParseMove_ParsesAttackState(string input, string expectedStart, string expectedEnd, AttackState expectedState)
        {
            var board = BoardState.DefaultPositions;
            var startSq = MoveParser.ParseSquare(expectedStart);
            var endSq = MoveParser.ParseSquare(expectedEnd);
            Move move;

            var success = MoveParser.TryParseMove(input, board, board.WhitePieces, out move);

            Assert.That(success, Is.True);

            Assert.That(move.StartRank, Is.EqualTo(startSq.Rank));
            Assert.That(move.StartFile, Is.EqualTo(startSq.File));
            Assert.That(move.EndRank, Is.EqualTo(endSq.Rank));
            Assert.That(move.EndFile, Is.EqualTo(endSq.File));
            Assert.That(move.AttackState, Is.EqualTo(expectedState));
        }

        [TestCase("a8=Q", "a7", "a8", PieceColor.White, SquareContents.White | SquareContents.Queen)]
        [TestCase("a1=Q", "a2", "a1", PieceColor.Black, SquareContents.Black | SquareContents.Queen)]
        [TestCase("a7", "a6", "a7", PieceColor.White, SquareContents.Empty)]
        [TestCase("a2", "a3", "a2", PieceColor.Black, SquareContents.Empty)]
        public void TryParseMove_ParsesPromotion(string input, string expectedStart, string expectedEnd, PieceColor color, SquareContents piece)
        {
            var board = BoardState.Empty;
            var endSq = MoveParser.ParseSquare(expectedEnd);
            var startSq = MoveParser.ParseSquare(expectedStart);
            var pieceColor = color == PieceColor.White ? SquareContents.White : SquareContents.Black;

            Move move;

            BoardStateManipulator.SetPiece(board, startSq, SquareContents.Pawn | pieceColor);
            var pieceMask = (pieceColor == SquareContents.White) ? board.WhitePieces : board.BlackPieces;

            var success = MoveParser.TryParseMove(input, board, pieceMask, out move);

            Assert.That(success, Is.True);

            Assert.That(move.StartRank, Is.EqualTo(startSq.Rank));
            Assert.That(move.StartFile, Is.EqualTo(startSq.File));
            Assert.That(move.EndRank, Is.EqualTo(endSq.Rank));
            Assert.That(move.EndFile, Is.EqualTo(endSq.File));
            Assert.That(move.PromotedPiece, Is.EqualTo(piece));
        }

        [TestCase("a8=P", "a7", SquareContents.Pawn | SquareContents.White)]
        [TestCase("a1=P", "a2", SquareContents.Pawn | SquareContents.Black)]
        [TestCase("a7=Q", "a6", SquareContents.Pawn | SquareContents.White)]
        [TestCase("a2=Q", "a3", SquareContents.Pawn | SquareContents.Black)]
        [TestCase("a8", "a7", SquareContents.Pawn | SquareContents.White)]
        [TestCase("a1", "a2", SquareContents.Pawn | SquareContents.Black)]
        [TestCase("Ra8=P", "a7", SquareContents.Rook | SquareContents.White)]
        [TestCase("Ra1=P", "a2", SquareContents.Rook | SquareContents.Black)]
        public void TryParseMove_InvalidPromotion_ReturnsFalse(string input, string expectedStart, SquareContents piece)
        {
            var board = BoardState.Empty;
            var startSq = MoveParser.ParseSquare(expectedStart);

            BoardStateManipulator.SetPiece(board, startSq, piece);
            var pieceMask = ((piece & SquareContents.Colours) == SquareContents.White) ? board.WhitePieces : board.BlackPieces;


            var success = MoveParser.TryParseMove(input, board, pieceMask, out _);

            Assert.That(success, Is.False);
        }

        [TestCase("Ra1a9", "a1", SquareContents.Rook | SquareContents.Rook)]
        [TestCase("Ra1i2", "a1", SquareContents.Rook | SquareContents.Rook)]
        [TestCase("Ra9a2", "a1", SquareContents.Rook | SquareContents.Rook)]
        [TestCase("Ri2a2", "a1", SquareContents.Rook | SquareContents.Rook)]
        public void TryParseMove_InvalidSquare_ReturnsFalse(string input, string expectedStart, SquareContents piece)
        {
            var board = BoardState.Empty;
            var startSq = MoveParser.ParseSquare(expectedStart);

            BoardStateManipulator.SetPiece(board, startSq, piece);
            var pieceMask = ((piece & SquareContents.Colours) == SquareContents.White) ? board.WhitePieces : board.BlackPieces;


            var success = MoveParser.TryParseMove(input, board, pieceMask, out _);

            Assert.That(success, Is.False);
        }


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

        [TestCase("a2a4", SquareContents.Pawn | SquareContents.White, "a2", "a4")]
        [TestCase("a2-a4", SquareContents.Pawn | SquareContents.White, "a2", "a4")]
        [TestCase("a3", SquareContents.Pawn | SquareContents.White, "a2", "a3")]
        [TestCase("a4", SquareContents.Pawn | SquareContents.White, "a2", "a4")]
        [TestCase("d5", SquareContents.Pawn | SquareContents.Black, "d7", "d5")]
        [TestCase("d6", SquareContents.Pawn | SquareContents.Black, "d7", "d6")]

        [TestCase("Kb1c2", SquareContents.King | SquareContents.White, "b1", "c2")]
        [TestCase("Kb1-c2", SquareContents.King | SquareContents.White, "b1", "c2")]
        [TestCase("Kc2", SquareContents.King | SquareContents.White, "b1", "c2")]

        [TestCase("Qb1d3", SquareContents.Queen | SquareContents.White, "b1", "d3")]
        [TestCase("Qb1-d3", SquareContents.Queen | SquareContents.White, "b1", "d3")]
        [TestCase("Qd3", SquareContents.Queen | SquareContents.White, "b1", "d3")]

        [TestCase("Rb1b4", SquareContents.Rook | SquareContents.White, "b1", "b4")]
        [TestCase("Rb1-b4", SquareContents.Rook | SquareContents.White, "b1", "b4")]
        [TestCase("Rb4", SquareContents.Rook | SquareContents.White, "b1", "b4")]

        [TestCase("Bb1d3", SquareContents.Bishop | SquareContents.White, "b1", "d3")]
        [TestCase("Bb1-d3", SquareContents.Bishop | SquareContents.White, "b1", "d3")]
        [TestCase("Bd3", SquareContents.Bishop | SquareContents.White, "b1", "d3")]

        [TestCase("Nb1c3", SquareContents.Knight | SquareContents.White, "b1", "c3")]
        [TestCase("Nb1-c3", SquareContents.Knight | SquareContents.White, "b1", "c3")]
        [TestCase("Nc3", SquareContents.Knight | SquareContents.White, "b1", "c3")]
        public void TryParseMove_GeneratesProperSquare_WhenMoving(string input, SquareContents piece, string expectedStart, string expectedEnd)
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


        [TestCase("a2xb3", SquareContents.Pawn | SquareContents.White, "a2", "b3")]
        [TestCase("axb3", SquareContents.Pawn | SquareContents.White, "a2", "b3")]
        [TestCase("Kb1xc2", SquareContents.King | SquareContents.White, "b1", "c2")]
        [TestCase("Kxc2", SquareContents.King | SquareContents.White, "b1", "c2")]
        [TestCase("Qb1xd3", SquareContents.Queen | SquareContents.White, "b1", "d3")]
        [TestCase("Qxd3", SquareContents.Queen | SquareContents.White, "b1", "d3")]
        [TestCase("Rb1xb4", SquareContents.Rook | SquareContents.White, "b1", "b4")]
        [TestCase("Rxb4", SquareContents.Rook | SquareContents.White, "b1", "b4")]
        [TestCase("Bb1xd3", SquareContents.Bishop | SquareContents.White, "b1", "d3")]
        [TestCase("Bxd3", SquareContents.Bishop | SquareContents.White, "b1", "d3")]
        [TestCase("Nb1xc3", SquareContents.Knight | SquareContents.White, "b1", "c3")]
        [TestCase("Nxc3", SquareContents.Knight | SquareContents.White, "b1", "c3")]
        public void TryParseMove_GeneratesProperSquare_WhenCapturing(string input, SquareContents piece, string expectedStart, string expectedEnd)
        {
            var board = BoardState.Empty;
            var endSq = MoveParser.ParseSquare(expectedEnd);
            var startSq = MoveParser.ParseSquare(expectedStart);
            Move move;

            var capturedPiece = (SquareContents.Pieces & piece) | (SquareContents.Colours & ~piece);

            BoardStateManipulator.SetPiece(board, startSq, piece);
            BoardStateManipulator.SetPiece(board, startSq, capturedPiece);
            var pieceMask = (piece & SquareContents.White) != 0 ? board.WhitePieces : board.BlackPieces;

            var success = MoveParser.TryParseMove(input, board, pieceMask, out move);

            Assert.That(success, Is.True);

            Assert.That(move.StartRank, Is.EqualTo(startSq.Rank));
            Assert.That(move.StartFile, Is.EqualTo(startSq.File));
            Assert.That(move.EndRank, Is.EqualTo(endSq.Rank));
            Assert.That(move.EndFile, Is.EqualTo(endSq.File));
        }

        // TODO: More tests
        [TestCase("8/8/8/8/8/8/8/R6R", "Rac1", SquareContents.Rook | SquareContents.White, "a1", "c1")]
        [TestCase("8/8/8/8/8/8/8/R6R", "Rhc1", SquareContents.Rook | SquareContents.White, "h1", "c1")]
        [TestCase("5R2/8/8/8/8/8/8/5R2", "R1f2", SquareContents.Rook | SquareContents.White, "f1", "f2")]
        [TestCase("5R2/8/8/8/8/8/8/5R2", "R8f2", SquareContents.Rook | SquareContents.White, "f8", "f2")]
        public void TryParseMove_GeneratesProperSquare_WithDisambiguation(string state, string input, SquareContents piece, string expectedStart, string expectedEnd)
        {
            var serializer = new FenSerializer();
            var board = serializer.Deserialize(state);
            var endSq = MoveParser.ParseSquare(expectedEnd);
            var startSq = MoveParser.ParseSquare(expectedStart);

            Move move;
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
