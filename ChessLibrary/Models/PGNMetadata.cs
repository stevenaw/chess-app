using System;

namespace ChessLibrary.Models
{
    public struct PGNMetadata
    {
        public string Event { get; set; }
        public string Site { get; set; }
        public DateTime DateTime { get; set; }
        public string Round { get; set; }
        public string White { get; set; }
        public string Black { get; set; }
        public string Result { get; set; }
        public string[] Moves {get;set;}
    }
}
