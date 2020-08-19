using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ChessLibrary.Tests.TestData
{
    public static class Scenarios
    {
        // https://www.chessgames.com/perl/chessgame?gid=1536488
        public const string Fischer_Altusky_1954 = "Fischer_Altusky_1954";

        // https://www.chessgames.com/perl/chessgame?gid=1008361
        public const string Fischer_Byrne_1956 = "Fischer_Byrne_1956";

        // https://www.chessgames.com/perl/chessgame?gid=1044399
        public const string Fischer_Spassky_1992_Game29 = "Fischer_Spassky_1992_Game29";

        // TODO: Read in from JSON file instead??
        public static ReadOnlyDictionary<string, string> FinalPositions = new ReadOnlyDictionary<string, string>(
            new Dictionary<string, string>()
            {
                { Fischer_Altusky_1954, "r4bnr/2p2kpp/p1np4/1p2p1q1/3PP1b1/8/PPP2PPP/RNBQK2R" },
                { Fischer_Byrne_1956, "1Q6/5pk1/2p3p1/1p2N2p/1b5P/1bn5/2r3P1/2K5" },
                {Fischer_Spassky_1992_Game29, "8/8/4R1p1/2k3p1/1p4P1/1P1b1P2/3K1n2/8" }
            }
        );
    }
}
