using System.Runtime.CompilerServices;

// ReSharper disable once CheckNamespace
namespace System.Collections.Generic
{
    /// <summary>
    /// Represents a doubly linked-list implemented with an array.
    /// The max number of items in the list = ushort.MaxValue - 1 = 2^16 - 2 = 65534.
    /// </summary>
    /// <typeparam name="T">The type of the items in the linked-list.</typeparam>
    public partial class ArrayLinkedList<T>
    {
        private const int DefaultCapacity = 16;
        private const ushort Nil = ushort.MaxValue;
        public const int MaxCapacity = ushort.MaxValue - 1;

        // contains the values
        private T[] _items;
        private ushort[] _previous;
        private ushort[] _next;
        private ushort _first;
        private ushort _last;
        private ushort _count;
        private int _version;

        public ArrayLinkedList(int capacity = DefaultCapacity)
        {
            if (capacity < 0 || capacity > MaxCapacity)
            {
                throw new ArgumentException($"Parameter {nameof(capacity)} should be within [0, {MaxCapacity}].", nameof(capacity));
            }

            _items = new T[capacity];
            _previous = new ushort[capacity];
            _next = new ushort[capacity];
            _first = _last = Nil;
        }

        public ArrayLinkedListNode<T> First
        {
            get
            {
                if (_count == 0) ThrowEmpty();
                return new ArrayLinkedListNode<T>(value: _items[_first], index: _first, _version);
            }
        }

        public ArrayLinkedListNode<T> Last
        {
            get
            {
                if (_count == 0) ThrowEmpty();
                return new ArrayLinkedListNode<T>(value: _items[_last], index: _last, _version);
            }
        }

        public ArrayLinkedListNode<T>? Find(T value)
        {
            if (_count == 0) return null;

            ushort current = _first;
            EqualityComparer<T> comparer = EqualityComparer<T>.Default;
            while (current != Nil)
            {
                T currentValue = _items[current];
                if (comparer.Equals(value, currentValue))
                {
                    return new ArrayLinkedListNode<T>(value: value, index: current, _version);
                }
                current = _next[current];
            }

            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ThrowEmpty()
        {
            throw new InvalidOperationException("Linked-list is empty.");
        }
    }
}