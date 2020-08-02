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
                    ("O-O", "e1", "g1"),
                    ("0-0", "e1", "g1"),
                    ("O-O-O", "e1", "c1"),
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
                var annotations = new AttackState[]
                {
                    AttackState.Check,
                    AttackState.Checkmate,
                    AttackState.None
                };
                var map = annotations.ToDictionary(
                    o => o,
                    o => MoveDescriptionHelper.GetAttackString(o)
                );

                var data = new[]
                {
                    ("O-O", "e1", "g1"),
                    ("0-0", "e1", "g1"),
                    ("O-O-O", "e1", "c1"),
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

            var success = MoveParser.TryParseMove(input, board, board.WhitePieces, out var move);

            Assert.That(success, Is.True);

            Assert.That(move.Move.StartRank, Is.EqualTo(startSq.Rank));
            Assert.That(move.Move.StartFile, Is.EqualTo(startSq.File));
            Assert.That(move.Move.EndRank, Is.EqualTo(endSq.Rank));
            Assert.That(move.Move.EndFile, Is.EqualTo(endSq.File));
            Assert.That(move.Annotation, Is.EqualTo(expectedState));
        }

        [TestCase("Nb1 c3")]
        [TestCase("Nb1 - c3")]
        public void TryParseMove_ReturnsFalseWhenInvalid(string input)
        {
            var board = BoardState.DefaultPositions;

            var success = MoveParser.TryParseMove(input, board, board.WhitePieces, out _);

            Assert.That(success, Is.False);
        }

        [TestCaseSource(nameof(AttackStateTestCases))]
        public void TryParseMove_ParsesAttackState(string input, string expectedStart, string expectedEnd, AttackState expectedState)
        {
            var board = BoardState.DefaultPositions;
            var startSq = MoveParser.ParseSquare(expectedStart);
            var endSq = MoveParser.ParseSquare(expectedEnd);

            var success = MoveParser.TryParseMove(input, board, board.WhitePieces, out var move);

            Assert.That(success, Is.True);

            Assert.That(move.Move.StartRank, Is.EqualTo(startSq.Rank));
            Assert.That(move.Move.StartFile, Is.EqualTo(startSq.File));
            Assert.That(move.Move.EndRank, Is.EqualTo(endSq.Rank));
            Assert.That(move.Move.EndFile, Is.EqualTo(endSq.File));
            Assert.That(move.AttackState, Is.EqualTo(expectedState));
        }

        [TestCase("a8=Q", "a7", "a8", PieceColor.White, SquareContents.White | SquareContents.Queen)]
        [TestCase("a1=Q", "a2", "a1", PieceColor.Black, SquareContents.Black | SquareContents.Queen)]
        [TestCase("a7", "a6", "a7", PieceColor.White, SquareContents.Empty)]
        [TestCase("a2", "a3", "a2", PieceColor.Black, SquareContents.Empty)]
        public void TryParseMove_ParsesPromotion(string input, string expectedStart, string expectedEnd, PieceColor color, SquareContents piece)
        {
            var endSq = MoveParser.ParseSquare(expectedEnd);
            var startSq = MoveParser.ParseSquare(expectedStart);
            var pieceColor = color == PieceColor.White ? SquareContents.White : SquareContents.Black;
            var board = BoardState.Empty.SetPiece(startSq, SquareContents.Pawn | pieceColor);

            var pieceMask = (pieceColor == SquareContents.White) ? board.WhitePieces : board.BlackPieces;

            var success = MoveParser.TryParseMove(input, board, pieceMask, out var move);

            Assert.That(success, Is.True);

            Assert.That(move.Move.StartRank, Is.EqualTo(startSq.Rank));
            Assert.That(move.Move.StartFile, Is.EqualTo(startSq.File));
            Assert.That(move.Move.EndRank, Is.EqualTo(endSq.Rank));
            Assert.That(move.Move.EndFile, Is.EqualTo(endSq.File));
            Assert.That(move.Move.PromotedPiece, Is.EqualTo(piece));
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
            var startSq = MoveParser.ParseSquare(expectedStart);
            var board = BoardState.Empty.SetPiece(startSq, piece);
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
            var startSq = MoveParser.ParseSquare(expectedStart);
            var board = BoardState.Empty.SetPiece(startSq, piece);

            var pieceMask = ((piece & SquareContents.Colours) == SquareContents.White) ? board.WhitePieces : board.BlackPieces;

            var success = MoveParser.TryParseMove(input, board, pieceMask, out _);

            Assert.That(success, Is.False);
        }


        [TestCase("O-O", PieceColor.White, "e1", "g1")]
        [TestCase("0-0", PieceColor.White, "e1", "g1")]
        [TestCase("O-O-O", PieceColor.White, "e1", "c1")]
        [TestCase("0-0-0", PieceColor.White, "e1", "c1")]
        [TestCase("Kd1", PieceColor.White, "e1", "d1")]
        [TestCase("Nc3", PieceColor.White, "b1", "c3")]

        [TestCase("O-O", PieceColor.Black, "e8", "g8")]
        [TestCase("0-0", PieceColor.Black, "e8", "g8")]
        [TestCase("O-O-O", PieceColor.Black, "e8", "c8")]
        [TestCase("0-0-0", PieceColor.Black, "e8", "c8")]
        [TestCase("Kd8", PieceColor.Black, "e8", "d8")]
        [TestCase("Nh6", PieceColor.Black, "g8", "h6")]
        public void TryParseMove_GeneratesProperSquare(string input, PieceColor color, string expectedStart, string expectedEnd)
        {
            var board = BoardState.DefaultPositions;
            var startSq = MoveParser.ParseSquare(expectedStart);
            var endSq = MoveParser.ParseSquare(expectedEnd);
            var activePieces = color == PieceColor.White ? board.WhitePieces : board.BlackPieces;

            var success = MoveParser.TryParseMove(input, board, activePieces, out var move);

            Assert.That(success, Is.True);

            Assert.That(move.Move.StartRank, Is.EqualTo(startSq.Rank));
            Assert.That(move.Move.StartFile, Is.EqualTo(startSq.File));
            Assert.That(move.Move.EndRank, Is.EqualTo(endSq.Rank));
            Assert.That(move.Move.EndFile, Is.EqualTo(endSq.File));
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
            var endSq = MoveParser.ParseSquare(expectedEnd);
            var startSq = MoveParser.ParseSquare(expectedStart);
            var board = BoardState.Empty.SetPiece(startSq, piece);

            var pieceMask = (piece & SquareContents.White) != 0 ? board.WhitePieces : board.BlackPieces;
            
            var success = MoveParser.TryParseMove(input, board, pieceMask, out var move);

            Assert.That(success, Is.True);

            Assert.That(move.Move.StartRank, Is.EqualTo(startSq.Rank));
            Assert.That(move.Move.StartFile, Is.EqualTo(startSq.File));
            Assert.That(move.Move.EndRank, Is.EqualTo(endSq.Rank));
            Assert.That(move.Move.EndFile, Is.EqualTo(endSq.File));
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
            var endSq = MoveParser.ParseSquare(expectedEnd);
            var startSq = MoveParser.ParseSquare(expectedStart);
            var capturedPiece = (SquareContents.Pieces & piece) | (SquareContents.Colours & ~piece);
            var board =  BoardState.Empty
                            .SetPiece(startSq, piece)
                            .SetPiece(startSq, capturedPiece);

            var pieceMask = (piece & SquareContents.White) != 0 ? board.WhitePieces : board.BlackPieces;

            var success = MoveParser.TryParseMove(input, board, pieceMask, out var move);

            Assert.That(success, Is.True);

            Assert.That(move.Move.StartRank, Is.EqualTo(startSq.Rank));
            Assert.That(move.Move.StartFile, Is.EqualTo(startSq.File));
            Assert.That(move.Move.EndRank, Is.EqualTo(endSq.Rank));
            Assert.That(move.Move.EndFile, Is.EqualTo(endSq.File));
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

            var pieceMask = (piece & SquareContents.White) != 0 ? board.WhitePieces : board.BlackPieces;
            
            var success = MoveParser.TryParseMove(input, board, pieceMask, out var move);

            Assert.That(success, Is.True);

            Assert.That(move.Move.StartRank, Is.EqualTo(startSq.Rank));
            Assert.That(move.Move.StartFile, Is.EqualTo(startSq.File));
            Assert.That(move.Move.EndRank, Is.EqualTo(endSq.Rank));
            Assert.That(move.Move.EndFile, Is.EqualTo(endSq.File));
        }

        [TestCase(FenSerializer.DefaultValue,   "e2,e3", '\0', AttackState.None, ExpectedResult = "e3", Description = "Pawn moves 1")]
        [TestCase(FenSerializer.DefaultValue,   "e2,e4", '\0', AttackState.None, ExpectedResult = "e4", Description = "Pawn moves 2")]
        [TestCase("8/8/8/4p3/3P4/8/8/K6k",      "d4,e5", '\0', AttackState.None, ExpectedResult = "dxe5", Description = "Regular pawn capture")]
        [TestCase("8/8/8/3Pp3/8/8/8/K6k",       "d5,e6", '\0', AttackState.None, ExpectedResult = "dxe6", Description = "En Passant")]
        [TestCase("8/P7/8/8/8/8/8/K6k",         "a7,a8", 'Q', AttackState.None, ExpectedResult = "a8=Q", Description = "Pawn promotion")]
        [TestCase("1n6/P7/8/8/8/8/8/K6k",       "a7,b8", 'Q', AttackState.None, ExpectedResult = "axb8=Q", Description = "Pawn promotion via capture")]
        [TestCase("1n5k/P7/8/8/8/8/8/K7",       "a7,b8", 'R', AttackState.Check, ExpectedResult = "axb8=R+", Description = "Pawn promotion via capture with check")]

        [TestCase(FenSerializer.DefaultValue,   "b1,c3", '\0', AttackState.None, ExpectedResult = "Nc3", Description = "Knight move regular")]
        [TestCase("8/8/8/4p3/8/3N4/8/K6k",      "d3,e5", '\0', AttackState.None, ExpectedResult = "Nxe5", Description = "Knight move capture")]
        [TestCase("3N1N2/8/8/8/8/8/8/K6k",      "d8,e6", '\0', AttackState.None, ExpectedResult = "Nde6", Description = "Knight move disambiguate file")]
        [TestCase("N7/8/N7/8/8/8/8/K6k",        "a8,c7", '\0', AttackState.None, ExpectedResult = "N8c7", Description = "Knight move disambiguate rank")]
        [TestCase("3N1N2/8/8/8/8/8/8/K6k",      "d8,e6", '\0', AttackState.None, ExpectedResult = "Nde6", Description = "Knight move disambiguate file")]
        [TestCase("N3N3/8/N7/8/8/8/8/K6k",      "a8,c7", '\0', AttackState.None, ExpectedResult = "Na8c7", Description = "Knight move disambiguate rank and file")]
        [TestCase("N3N3/8/N7/8/4k3/8/8/K7",      "a8,c7", '\0', AttackState.Check, ExpectedResult = "Na8c7+", Description = "Knight move disambiguate rank and file with check")]

        [TestCase("r3k2r/8/8/8/8/8/8/R3K2R",    "e1,d2", '\0', AttackState.None, ExpectedResult = "Kd2", Description = "King move regular")]
        [TestCase("r3k2r/8/8/8/8/8/8/R3K2R",    "e1,g1", '\0', AttackState.None, ExpectedResult = "0-0", Description = "Castle kingside white")]
        [TestCase("r3k2r/8/8/8/8/8/8/R3K2R",    "e1,c1", '\0', AttackState.None, ExpectedResult = "0-0-0", Description = "Castle queenside white")]
        [TestCase("r3k2r/8/8/8/8/8/8/R3K2R",    "e8,g8", '\0', AttackState.None, ExpectedResult = "0-0", Description = "Castle kingside black")]
        [TestCase("r3k2r/8/8/8/8/8/8/R3K2R",    "e8,c8", '\0', AttackState.None, ExpectedResult = "0-0-0", Description = "Castle queenside black")]
        [TestCase("r4k1r/8/8/8/8/8/8/R3K2R",    "e1,g1", '\0', AttackState.Check, ExpectedResult = "0-0+", Description = "Castle kingside with check")]
        [TestCase("r2k3r/8/8/8/8/8/8/R3K2R",    "e1,c1", '\0', AttackState.Check, ExpectedResult = "0-0-0+", Description = "Castle queenside with check")]
        [TestCase("r4k1r/8/8/8/8/8/8/R3K2R",    "e1,g1", '\0', AttackState.Checkmate, ExpectedResult = "0-0#", Description = "Castle kingside with check")]
        [TestCase("r2k3r/8/8/8/8/8/8/R3K2R",    "e1,c1", '\0', AttackState.Checkmate, ExpectedResult = "0-0-0#", Description = "Castle queenside with checkmate")]
        public string ToMoveString_SuccessCases(string fen, string moveStr, char promotedPieceToken, AttackState attackState)
        {
            var serializer = new FenSerializer();
            var board = serializer.Deserialize(fen);

            var squares = moveStr.Split(',');
            var start = MoveParser.ParseSquare(squares[0]);
            var end = MoveParser.ParseSquare(squares[1]);
            var promotedPiece = promotedPieceToken != 0 ? FenSerializer.FromNotation(promotedPieceToken) : SquareContents.Empty;

            var move = new Move(start.File, start.Rank, end.File, end.Rank, promotedPiece);

            var result = MoveParser.ToMoveString(move, board, attackState);
            var resultStr = result.ToString();

            return resultStr;
        }
    }
}
