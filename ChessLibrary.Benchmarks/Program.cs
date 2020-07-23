using BenchmarkDotNet.Running;

namespace ChessLibrary.Benchmarks
{
    class Program
    {
        static void Main()
        {
            BenchmarkRunner.Run<ParsingBenchmarks_Castling>();
        }
    }
}
