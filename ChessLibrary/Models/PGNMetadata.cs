using System;

namespace ChessLibrary.Models
{
    public class PGNMetadata
    {
        public string Event { get; set; } = string.Empty;
        public string Site { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public string Round { get; set; } = string.Empty;
        public string White { get; set; } = string.Empty;
        public string Black { get; set; } = string.Empty;
        public string Result { get; set; } = string.Empty;
        public string[] Moves { get; set; } = Array.Empty<string>();
    }
}
