using BenchmarkDotNet.Attributes;
using System;

namespace ChessLibrary.Benchmarks
{
    [MemoryDiagnoser]
    public class ParsingBenchmarks_Castling
    {
        ReadOnlyMemory<char>[] TestCases = new ReadOnlyMemory<char>[]
        {
            "0-0".AsMemory(),
            "O-O".AsMemory(),
            "0-0-0".AsMemory(),
            "O-O-O".AsMemory()
        };

        [Benchmark(Baseline = true, OperationsPerInvoke = 4)]
        public bool Original()
        {
            Original_Impl(TestCases[3].Span);
            Original_Impl(TestCases[2].Span);
            Original_Impl(TestCases[1].Span);
            return Original_Impl(TestCases[0].Span);
        }

        [Benchmark(OperationsPerInvoke = 4)]
        public bool ManualCompare()
        {
            ManualCompare_Impl(TestCases[3].Span);
            ManualCompare_Impl(TestCases[2].Span);
            ManualCompare_Impl(TestCases[1].Span);
            return ManualCompare_Impl(TestCases[0].Span);
        }


        private static bool Original_Impl(ReadOnlySpan<char> moveNotation)
        {
            var castleKingside = moveNotation.Equals("O-O".AsSpan(), StringComparison.Ordinal)
                || moveNotation.Equals("0-0".AsSpan(), StringComparison.Ordinal);
            var castleQueenside = moveNotation.Equals("O-O-O".AsSpan(), StringComparison.Ordinal)
                || moveNotation.Equals("0-0-0".AsSpan(), StringComparison.Ordinal);

            return (castleKingside || castleQueenside);
        }

        private static bool ManualCompare_Impl(ReadOnlySpan<char> moveNotation)
        {
            var potentialCastling = moveNotation.Length == 3 || moveNotation.Length == 5;

            potentialCastling = potentialCastling
                    && moveNotation[2] == moveNotation[0]
                    && moveNotation[1] == '-'
                    && (moveNotation[0] == '0' || moveNotation[0] == 'O');

            if (potentialCastling && moveNotation.Length == 5)
            {
                potentialCastling = moveNotation[4] == moveNotation[0]
                    && moveNotation[3] == '-';
            }

            return potentialCastling;
        }
    }
}
