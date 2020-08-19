using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ChessLibrary.Tests.TestData
{
    public static class Scenarios
    {
        public const string Fischer_Altusky_1954 = "Fischer_Altusky_1954";
        public const string Fischer_Byrne_1956 = "Fischer_Byrne_1956";
        public const string Fischer_Spassky_1992_Game29 = "Fischer_Spassky_1992_Game29";

        public static class MatingScenarios
        {
            public const string MateByPromotion = "Wiede_Goetz_1880_PromotionMate";
            public const string MateByCastling = "Kvicala_NN_1869_CastlingMate";

            public static readonly string[] All = new[]
            {
                MateByPromotion,
                MateByCastling
            };
        }

        // TODO: Read in from JSON file instead??
        public static ReadOnlyDictionary<string, string> FinalPositions = new ReadOnlyDictionary<string, string>(
            new Dictionary<string, string>()
            {
                // Sanity Checks
                //--------------
                // https://www.chessgames.com/perl/chessgame?gid=1536488
                { Fischer_Altusky_1954, "r4bnr/2p2kpp/p1np4/1p2p1q1/3PP1b1/8/PPP2PPP/RNBQK2R" },
                // https://www.chessgames.com/perl/chessgame?gid=1008361
                { Fischer_Byrne_1956, "1Q6/5pk1/2p3p1/1p2N2p/1b5P/1bn5/2r3P1/2K5" },
                // https://www.chessgames.com/perl/chessgame?gid=1044399
                { Fischer_Spassky_1992_Game29, "8/8/4R1p1/2k3p1/1p4P1/1P1b1P2/3K1n2/8" },

                // Special Mating Scenarios
                //-------------------------
                // https://www.chessgames.com/perl/chessgame?gid=1075778
                { MatingScenarios.MateByPromotion, "rnb1kbnr/pppp1ppp/8/8/4q3/1P5P/P1PP1K2/RNBQ1BNn" },
                // https://www.chessgames.com/perl/chessgame?gid=1272123
                { MatingScenarios.MateByCastling, "r1b4r/2q3b1/p2kpQ1p/2p1N3/2p1p3/8/PPP2PPP/2KR3R" }
            }
        );
    }
}
