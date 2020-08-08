using ChessLibrary.Models;
using System;
using System.IO;
using System.Linq;

namespace ChessLibrary.Serialization
{
    class PGNSerializer
    {
        private const int LineLength = 80;

        public static class WellKnownTags
        {
            public const string Event = "Event";
            public const string Site = "Site";
            public const string Date = "Date";
            public const string Round = "Round";
            public const string White = "White";
            public const string Black = "Black";
            public const string Result = "Result";
        }

        

        public void Serialize(Game game, PGNMetadata metadata, TextWriter writer)
        {
            var date = $"{metadata.DateTime.Year}.{metadata.DateTime.Month.ToString("00")}.{metadata.DateTime.Day.ToString("00")}";
            // TODO: Async I/O
            writer.WriteLine($"[{WellKnownTags.Event} \"{metadata.Event}\"]");
            writer.WriteLine($"[{WellKnownTags.Site} \"{metadata.Site}\"]");
            writer.WriteLine($"[{WellKnownTags.Date} \"{date}\"]");
            writer.WriteLine($"[{WellKnownTags.Round} \"{metadata.Round}\"]");
            writer.WriteLine($"[{WellKnownTags.White} \"{metadata.White}\"]");
            writer.WriteLine($"[{WellKnownTags.Black} \"{metadata.Black}\"]");
            writer.WriteLine($"[{WellKnownTags.Result} \"{metadata.Result}\"]");
            writer.WriteLine();

            // TODO: Better way to reverse
            var history = game.History.ToArray().Reverse().ToArray();

            var moves = new (Move move, BoardState board, AttackState attack)[history.Length - 1];
            for (var i = 0; i < history.Length-1; i++)
                moves[i] = (history[i + 1].PrecedingMove, history[i].Board, history[i].AttackState);

            var linePos = 0;
            for (var i = 0; i < moves.Length; i ++)
            {
                var ply = MoveParser.ToMoveString(moves[i].move, moves[i].board, moves[i].attack);
                if (ply.StartsWith('0'))
                    ply = ply.Replace('0', 'O');

                var moveNumber = string.Empty;

                if (i % 2 == 0)
                    moveNumber = ((i/2) + 1).ToString() + ". ";

                var lengthToWrite = ply.Length + moveNumber.Length;
                if (linePos + lengthToWrite + 1 > LineLength)
                {
                    writer.WriteLine();
                    linePos = 0;
                }
                else if (i != 0)
                {
                    writer.Write(' ');
                    linePos++;
                }

                writer.Write(moveNumber);
                writer.Write(ply);
                linePos += lengthToWrite;
            }

            if (linePos + metadata.Result.Length + 1 > LineLength)
                writer.WriteLine();
            else
                writer.Write(' ');

            writer.WriteLine(metadata.Result);
        }
    }
}
