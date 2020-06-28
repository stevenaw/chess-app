using System.Runtime.CompilerServices;

namespace ChessLibrary.Models
{
    internal struct IndexedTuple<T> where T : struct
    {
        private T _first;
#pragma warning disable IDE0044 // Add readonly modifier
        private T _second;
#pragma warning restore IDE0044 // Add readonly modifier

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Get(PieceColor colour) => GetWithPointerArithmetic((int)colour);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Get(int idx) => GetWithPointerArithmetic(idx);

        private unsafe T GetWithPointerArithmetic(int idx)
        {
            var first = Unsafe.AsPointer(ref _first);
            var target = Unsafe.Add<T>(first, idx);
            return Unsafe.Read<T>(target);
        }

        private static IndexedTuple<T> _empty = new IndexedTuple<T>(default, default);
        public static IndexedTuple<T> Empty
        {
            get { return _empty; }
        }
    }
}
