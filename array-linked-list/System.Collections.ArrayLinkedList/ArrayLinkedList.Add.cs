// ReSharper disable once CheckNamespace
namespace System.Collections.Generic
{
    public partial class ArrayLinkedList<T>
    {
        public void AddFirst(T value)
        {
            if (_count > 0)
            {
                GrowIfNeeded();

                _items[_count] = value;
                _previous[_first] = _count;
                _previous[_count] = Nil;
                _next[_count] = _first;
                _first = _count;
            }
            else
            {
                AddWhenEmpty(value);
            }

            _count++;
            _version++;
        }

        public void AddLast(T value)
        {
            if (_count > 0)
            {
                GrowIfNeeded();

                _items[_count] = value;
                _next[_last] = _count;
                _previous[_count] = _last;
                _next[_count] = Nil;
                _last = _count;
            }
            else
            {
                AddWhenEmpty(value);
            }

            _count++;
            _version++;
        }

        private void GrowIfNeeded()
        {
            if (_count < _items.Length) return;

            T[] newItems = new T[_items.Length * 2];
            ushort[] newPrevious = new ushort[_previous.Length * 2];
            ushort[] newNext = new ushort[_next.Length * 2];

            // move the items and compound indices into the new arrays by rearranging them
            ushort current = _first;
            for (ushort i = 0; i < _count; ++i)
            {
                newItems[i] = _items[current];
                newPrevious[i] = (ushort)(i - 1);
                newNext[i] = (ushort)(i + 1);
                current = _next[current];
            }

            // fix first's previous index and last's next index
            newPrevious[0] = Nil;
            newNext[_count - 1] = Nil;

            _items = newItems;
            _previous = newPrevious;
            _next = newNext;
            _first = 0;
            _last = (ushort)(_count - 1);
        }

        private void AddWhenEmpty(T value)
        {
            _items[0] = value;
            _previous[0] = Nil;
            _next[0] = Nil;
            _first = _last = 0;
        }
    }
}