using BenchmarkDotNet.Running;
using System;

namespace ChessLibrary.Benchmarks
{
    class Program
    {
        static void Main()
        {
            BenchmarkRunner.Run<IndexedTupleBenchmarks>();
        }
    }
}
