using BenchmarkDotNet.Attributes;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Text;

namespace ChessLibrary.Benchmarks
{
    [MemoryDiagnoser, ShortRunJob]
    public class BoardStateEqualsBenchmarks
    {
        private readonly BoardState A = new BoardState(0);
        private readonly BoardState B = new BoardState(0);

        private static readonly Func<object, object, bool> InternalFastCheck = GetFastCheck();

        [Benchmark(Baseline = true)]
        public bool DefaultEquals()
        {
            return A.Equals(B);
        }

        [Benchmark]
        public bool InternalsEquals()
        {
            return InternalFastCheck(A, B);
        }

        [Benchmark]
        public bool CustomEquals()
        {
            return MyEquals(A, B);
        }

        [Benchmark]
        public bool CustomEqualsWithMemcmp()
        {
            return MyEqualsWithMemcmp(A, B);
        }

        [Benchmark]
        public bool CustomEqualsWithVector()
        {
            return MyEqualsWithVector(A, B);
        }

        [Benchmark]
        public bool CustomEqualsWithPropCmp()
        {
            return MyEqualsWithPropCmp(A, B);
        }

        public static Func<object, object, bool> GetFastCheck()
        {
            // Used internally by ValueType::Equals for tightly-packed structs
            //https://github.com/dotnet/coreclr/blob/5b1c001/src/System.Private.CoreLib/src/System/ValueType.cs#L42-L43
            //https://github.com/dotnet/coreclr/blob/e9ac7dc62becb08463a3d5a71f02eb247b1727c7/src/vm/comutilnative.cpp#L1946-L1948
            //https://github.com/dotnet/coreclr/blob/e9ac7dc62becb08463a3d5a71f02eb247b1727c7/src/vm/comutilnative.cpp#L1966
            var member = typeof(ValueType).GetMethod(
                "FastEqualsCheck",
                BindingFlags.NonPublic | BindingFlags.Static
            );
            return (Func<object, object, bool>)member.CreateDelegate(typeof(Func<object, object, bool>));
        }

        private static bool MyEqualsWithPropCmp(BoardState a, BoardState b)
        {
            return a.Bishops == b.Bishops
                && a.BlackPieces == b.BlackPieces
                && a.Kings == b.Kings
                && a.Knights == b.Knights
                && a.Pawns == b.Pawns
                && a.Queens == b.Queens
                && a.Rooks == b.Rooks
                && a.WhitePieces == b.WhitePieces;
        }

        private static bool MyEquals(BoardState a, BoardState b)
        {
            Span<BoardState> buffer = stackalloc BoardState[2];
            buffer[0] = a;
            buffer[1] = b;

            var bytes = MemoryMarshal.AsBytes(buffer);
            return bytes.Slice(0, bytes.Length / 2).SequenceEqual(bytes.Slice(bytes.Length / 2));
        }

        private static unsafe bool MyEqualsWithVector(BoardState a, BoardState b)
        {
            Span<BoardState> buffer = stackalloc BoardState[2];
            buffer[0] = a;
            buffer[1] = b;

            var bytes = MemoryMarshal.Cast<BoardState, long>(buffer);
            fixed (long* aptr = bytes.Slice(0, bytes.Length / 2))
            {
                fixed (long* bptr = bytes.Slice(bytes.Length / 2))
                {
                    long* rptr = stackalloc long[4];
                    for (var i = 0; i <= 64; i += 8)
                    {
                        var av = Avx2.LoadVector256(aptr + i);
                        var bv = Avx2.LoadVector256(bptr + i);
                        var compVector = Avx2.CompareEqual(av, bv);
                        Avx2.Store(rptr, compVector);

                        if (rptr[0] != -1 || rptr[1] != -1 || rptr[2] != -1 || rptr[3] != -1)
                            return false;
                    }
                }
            }
            return true;
        }

        private static unsafe bool MyEqualsWithMemcmp(BoardState a, BoardState b)
        {
            Span<BoardState> buffer = stackalloc BoardState[2];
            buffer[0] = a;
            buffer[1] = b;

            var bytes = MemoryMarshal.AsBytes(buffer);
            fixed(byte* aptr = bytes.Slice(0, bytes.Length / 2))
            {
                fixed (byte* bptr = bytes.Slice(bytes.Length / 2))
                {
                    return memcmp(aptr, bptr, bytes.Length / 2) == 0;
                }
            }
        }

        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        private static unsafe extern int memcmp(byte* b1, byte* b2, int count);
    }


    internal readonly struct BoardState
    {
        public BoardState(int i)
        {
            WhitePieces = 0b_1111_1111_1111_1111;
            Pawns = 0b_1111_1111_0000_0000;
            Rooks = 0b_1000_0001;
            Knights = 0b_0100_0010;
            Bishops = 0b_0010_0100;
            Queens = 0b_0000_1000;
            Kings = 0b_0001_0000;

            BlackPieces = (ulong)0b_1111_1111_1111_1111 << (8 * 6);
            Pawns |= Pawns << (8 * 5);
            Rooks |= Rooks << (8 * 7);
            Knights |= Knights << (8 * 7);
            Bishops |= Bishops << (8 * 7);
            Queens |= Queens << (8 * 7);
            Kings |= Kings << (8 * 7);
        }

        public ulong WhitePieces { get; }
        public ulong BlackPieces { get; }

        public ulong Pawns { get; }
        public ulong Bishops { get; }
        public ulong Knights { get; }
        public ulong Rooks { get; }
        public ulong Kings { get; }
        public ulong Queens { get; }
    }
}
