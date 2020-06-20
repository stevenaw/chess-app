using BenchmarkDotNet.Attributes;
using System;
using System.Runtime.CompilerServices;

namespace ChessLibrary.Benchmarks
{
    [MemoryDiagnoser, ShortRunJob]
    public class IndexedTupleBenchmarks
    {
        private IndexedTuple<int> Target = new IndexedTuple<int>(4, 42);

        [Params(0, 1)]
        public int Idx { get; set; }

        [Benchmark]
        public int IndexedTuple_Indexed() { return Target.GetWithPointerArithmetic(Idx); }

        [Benchmark]
        public int IndexedTuple_Conditional() { return Target.GetWithConditional(Idx); }

        [Benchmark]
        public int IndexedTuple_Case() { return Target.GetWithCase(Idx); }


        private struct IndexedTuple<T>
        {
            private T _first;
            private T _second;

            public T First
            {
                get { return _first; }
            }

            public T Second
            {
                get { return _second; }
            }

            public IndexedTuple(T first, T second)
            {
                _first = first;
                _second = second;
            }

            public T GetWithConditional(int idx)
            {
                return idx == 0 ? First : Second;
            }

            public T GetWithCase(int idx)
            {
                switch(idx)
                {
                    case 0:
                        return First;
                    case 1:
                    default:
                        return Second;
                }
            }

            public unsafe T GetWithPointerArithmetic(int idx)
            {
                var first = Unsafe.AsPointer(ref _first);
                var target = Unsafe.Add<T>(first, idx);
                return Unsafe.Read<T>(target);
            }
        }
    }
}
