using System.Collections.ObjectModel;

namespace ChessLibrary.Tests.TestData
{
    public static class Scenarios
    {
        public static readonly string Fischer_Altusky_1954 = "Fischer_Altusky_1954";
        public static readonly string Fischer_Byrne_1956 = "Fischer_Byrne_1956";
        public static readonly string Fischer_Spassky_1992_Game29 = "Fischer_Spassky_1992_Game29";

        public static class MatingScenarios
        {
            public static readonly string MateByPromotion = "Wiede_Goetz_1880_MatePromotion";
            public static readonly string MateByCastling = "Kvicala_NN_1869_MateCastling";
            public static readonly string MateByDiscovery = "Lasker_Thomas_1912_MateDiscovery";
            public static readonly string MateByEnPassant = "Korepanova_Tishkov_2007_MateEnPassant";
            public static readonly string MateByEnPassantDiscovery = "Gundersen_Faul_1928_MateEnPassantDiscovery";

            public static readonly string[] All =
            [
                MateByPromotion,
                MateByCastling,
                MateByDiscovery,
                MateByEnPassant,
                MateByEnPassantDiscovery
            ];
        }

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
                { MatingScenarios.MateByCastling, "r1b4r/2q3b1/p2kpQ1p/2p1N3/2p1p3/8/PPP2PPP/2KR3R" },
                // https://www.chessgames.com/perl/chessgame?gid=1259009
                { MatingScenarios.MateByDiscovery, "rn3r2/pbppq1p1/1p2pN2/8/3P2NP/6P1/PPPKBP1R/R5k1" },
                // https://www.chessgames.com/perl/chessgame?gid=1886010
                { MatingScenarios.MateByEnPassant, "5r2/1n6/2p3k1/b1Pb3p/P2P2pP/Q3PpP1/1B4KR/4q3" },
                // https://www.chessgames.com/perl/chessgame?gid=1242924
                { MatingScenarios.MateByEnPassantDiscovery, "r1bq1r2/pp2n3/4N1Pk/3pPp2/1b1n2Q1/2N5/PP3PP1/R1B1K2R" },

                // Other unique situations
                // -----------------------
                // https://www.chessgames.com/perl/chessgame?gid=1316514  // 5 Queens in a game
                { "Belov_Prohorov_1991", "8/7k/6bp/8/4P1Q1/8/8/2K1q3" },
                // https://www.chessgames.com/perl/chessgame?gid=1268705  // Longest game
                { "Nikolic_Arsovic_1989", "8/6r1/8/8/3K4/3B4/5R2/3k4" },
                // https://www.chessgames.com/perl/chessgame?gid=1297307  // Longest decisive game
                { "Stepak_Mashian_1980", "8/5P1q/4K3/4Q3/8/8/5k2/8" },
            }
        );
    }
}
