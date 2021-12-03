// ReSharper disable once CheckNamespace
namespace System.Collections.Generic
{
    public partial class ArrayLinkedList<T> : IReadOnlyCollection<T>, ICollection<T>
    {
        public void Add(T item)
        {
            AddLast(item);
        }

        public void Clear()
        {
            for (int i = 0; i < _count; ++i)
            {
                _items[i] = default;
                _previous[i] = default;
                _next[i] = default;
            }

            _first = _last = Nil;
            _count = 0;
            _version++;
        }

        public bool Contains(T item)
        {
            ArrayLinkedListNode<T>? node = Find(item);
            return node.HasValue;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            ushort current = _first;
            while (current != Nil)
            {
                T value = _items[current];
                array[arrayIndex] = value;
                arrayIndex++;

                current = _next[current];
            }
        }

        public bool Remove(T item)
        {
            ArrayLinkedListNode<T>? node = Find(item);
            if (!node.HasValue) return false;

            ushort removed = node.Value.Index;
            if (_previous[removed] == Nil)
            {
                RemoveFirst();
            }
            else if (_next[removed] == Nil)
            {
                RemoveLast();
            }
            else
            {
                RemoveInternalNode(removed);
            }

            return true;
        }

        public int Count => _count;

        public bool IsReadOnly => false;

        public IEnumerator<T> GetEnumerator()
        {
            if (_count == 0) yield break;

            int initialVersion = _version;
            ushort current = _first;
            while (current != Nil)
            {
                yield return _items[current];
                current = _next[current];
                if (_version != initialVersion)
                {
                    throw new InvalidOperationException("Collection was modified while it is enumerated.");
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}