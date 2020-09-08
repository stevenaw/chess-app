using System.Collections.Generic;

namespace ChessLibrary.Models
{
    public struct PGNMetadata
    {
        public string Event { get; set; }
        public string Site { get; set; }
        public string Date { get; set; }
        public string Round { get; set; }
        public string White { get; set; }
        public string Black { get; set; }
        public string Result { get; set; }
        public List<string> Moves { get; set; }
    }
}
