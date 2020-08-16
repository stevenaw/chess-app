using ChessLibrary.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ChessLibrary.Serialization
{
    class PGNSerializer
    {
        // 64 chars per line (2 for line ending)
        private const int LineLength = 62;

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

        public async Task Serialize(PGNMetadata pgn, TextWriter writer)
        {
            // TODO: Async I/O
            await writer.WriteLineAsync($"[{WellKnownTags.Event} \"{pgn.Event}\"]");
            await writer.WriteLineAsync($"[{WellKnownTags.Site} \"{pgn.Site}\"]");
            await writer.WriteLineAsync($"[{WellKnownTags.Date} \"{pgn.Date}\"]");
            await writer.WriteLineAsync($"[{WellKnownTags.Round} \"{pgn.Round}\"]");
            await writer.WriteLineAsync($"[{WellKnownTags.White} \"{pgn.White}\"]");
            await writer.WriteLineAsync($"[{WellKnownTags.Black} \"{pgn.Black}\"]");
            await writer.WriteLineAsync($"[{WellKnownTags.Result} \"{pgn.Result}\"]");
            await writer.WriteLineAsync();

            var linePos = 0;
            for (var i = 0; i < pgn.Moves.Count; i++)
            {
                var moveNumber = string.Empty;
                if (i % 2 == 0)
                    moveNumber = ((i / 2) + 1).ToString() + ". ";

                var move = pgn.Moves[i];
                var lengthToWrite = move.Length + moveNumber.Length;
                if (linePos + lengthToWrite + 1 > LineLength)
                {
                    await writer.WriteLineAsync();
                    linePos = 0;
                }
                else if (i != 0)
                {
                    await writer.WriteAsync(' ');
                    linePos++;
                }

                await writer.WriteAsync(moveNumber);
                await writer.WriteAsync(move);
                linePos += lengthToWrite;
            }

            if (linePos + pgn.Result.Length + 1 > LineLength)
                await writer.WriteLineAsync();
            else
                await writer.WriteAsync(' ');

            await writer.WriteAsync(pgn.Result);
        }

        public async Task<PGNMetadata> Deserialize(TextReader reader)
        {
            var pgn = new PGNMetadata();

            string? currentLine = await reader.ReadLineAsync();
            while (!string.IsNullOrEmpty(currentLine))
            {
                // Parse tags
                // VERY very rudimentary. A proper parser would account for leading/trailing whitespan on tags, tags spanning multiple lines, multiple tags on a line
                if (TryParseTag(currentLine, out var tag))
                {
                    switch(tag.tagName)
                    {
                        case WellKnownTags.Date:
                            pgn.Date = tag.tagValue;
                            break;
                        case WellKnownTags.Event:
                            pgn.Event = tag.tagValue;
                            break;
                        case WellKnownTags.Result:
                            pgn.Result = tag.tagValue;
                            break;
                        case WellKnownTags.Round:
                            pgn.Round = tag.tagValue;
                            break;
                        case WellKnownTags.Site:
                            pgn.Site = tag.tagValue;
                            break;
                        case WellKnownTags.White:
                            pgn.White = tag.tagValue;
                            break;
                        case WellKnownTags.Black:
                            pgn.Black = tag.tagValue;
                            break;
                    }
                }

                currentLine = await reader.ReadLineAsync();
            }

            // Consume blank lines before moveset
            while (currentLine?.Length == 0)
                currentLine = await reader.ReadLineAsync();

            // Parse movetext
            var moves = new List<string>();
            bool insideComment = false;
            while (!string.IsNullOrEmpty(currentLine))
            {
                var movesOnLine = ParseLineToMoves(currentLine, ref insideComment);
                moves.AddRange(movesOnLine);

                currentLine = await reader.ReadLineAsync();
            }
            pgn.Moves = moves;

            return pgn;
        }

        private static List<string> ParseLineToMoves(string line, ref bool insideComment)
        {
            var moves = new List<string>();

            var lineSpan = line.AsSpan().Trim();

            if (insideComment == true)
            {
                var endOfComment = lineSpan.IndexOf('}');
                if (endOfComment == -1)
                    return moves;

                var nextTokenStart = Math.Min(endOfComment + 2, lineSpan.Length);
                lineSpan = lineSpan.Slice(nextTokenStart);
                insideComment = false;
            }


            while(lineSpan.Length > 0)
            {
                var endOfToken = lineSpan.IndexOf(' ');
                if (endOfToken == -1)
                    endOfToken = lineSpan.Length;

                var token = lineSpan.Slice(0, endOfToken);
                var nextTokenStart = Math.Min(endOfToken + 1, lineSpan.Length);

                lineSpan = lineSpan.Slice(nextTokenStart);

                // TODO: Ignore ';' comments
                if (token[0] == '{')
                {
                    if (token[token.Length - 1] == '}')
                        continue;
                    else
                    {
                        var endOfComment = lineSpan.IndexOf('}');
                        if (endOfComment != -1)
                        {
                            nextTokenStart = Math.Min(endOfComment + 2, lineSpan.Length);
                            lineSpan = lineSpan.Slice(nextTokenStart);
                        }
                        else
                        {
                            lineSpan = lineSpan.Slice(lineSpan.Length);
                            insideComment = true;
                        }
                    }
                }
                else if (Char.IsLetter(token[0]))
                {
                    // TODO: Simple verification of if algebraic notation string
                    moves.Add(token.ToString());
                }
            }


            return moves;
        }

        private static bool TryParseTag(string line, out (string tagName, string tagValue) result)
        {
            // http://www.saremba.de/chessgml/standards/pgn/pgn-complete.htm#c8.1
            if (string.IsNullOrEmpty(line) || line.Length < 6 || line[0] != '[' || line[line.Length - 1] != ']')
            {
                result = (string.Empty, string.Empty);
                return false;
            }

            var lineSpan = line.AsSpan().Slice(1);
            // Get the tag name
            var i = 0;
            while (Char.IsLetterOrDigit(lineSpan[i]) || lineSpan[i] == '_')
                i++;
            var tagName = lineSpan.Slice(0, i).ToString();

            // Find start of tag value
            while (Char.IsWhiteSpace(lineSpan[i]))
                i++;

            if (lineSpan[i] != '"')
            {
                result = (string.Empty, string.Empty);
                return false;
            }

            lineSpan = lineSpan.Slice(++i);
            var endOfTagValue = lineSpan.IndexOf('"');

            if (endOfTagValue < 0)
            {
                result = (string.Empty, string.Empty);
                return false;
            }

            var tagValue = lineSpan.Slice(0, endOfTagValue).ToString();
            result = (tagName, tagValue);
            return true;
        }
    }
}
