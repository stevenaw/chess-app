using BenchmarkDotNet.Running;
using System;

namespace ChessLibrary.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<BoardStateEqualsBenchmarks>();
        }
    }
}
