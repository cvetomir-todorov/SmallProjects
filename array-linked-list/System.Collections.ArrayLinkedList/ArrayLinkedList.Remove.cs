using System.Diagnostics;

// ReSharper disable once CheckNamespace
namespace System.Collections.Generic
{
    public partial class ArrayLinkedList<T>
    {
        public bool TryRemoveFirst(out T value)
        {
            if (_count == 0)
            {
                value = default;
                return false;
            }

            value = DoRemoveFirst();
            return true;
        }

        public T RemoveFirst()
        {
            if (_count == 0) ThrowEmpty();
            return DoRemoveFirst();
        }

        private T DoRemoveFirst()
        {
            T firstValue = _items[_first];
            ushort firstItemChild = _next[_first];

            if (_first != _count - 1)
            {
                ushort movedItemParent = _previous[_count - 1];
                ushort movedItemChild = _next[_count - 1];

                // move the item at the end in place of the removed first item
                _items[_first] = _items[_count - 1];
                _previous[_first] = movedItemParent;
                _next[_first] = movedItemChild;

                // update the moved item's parent
                if (movedItemParent != Nil && movedItemParent != _first)
                {
                    _next[movedItemParent] = _first;
                }

                // update the moved item's child
                if (movedItemChild != Nil && movedItemChild != _first)
                {
                    _previous[movedItemChild] = _first;
                }

                // update last index if it is the one being moved
                if (_last == _count - 1)
                {
                    _last = _first;
                }
            }

            _items[_count - 1] = default;
            _previous[_count - 1] = default;
            _next[_count - 1] = default;

            if (_count == 1)
            {
                _first = _last = Nil;
            }
            else if (_count == 2)
            {
                _first = _last = 0;
                _previous[0] = _next[0] = Nil;
            }
            else
            {
                // if first item's child is the one being moved then we don't need to update first
                if (firstItemChild != _count - 1)
                {
                    _first = firstItemChild;
                }
                _previous[_first] = Nil;
            }

            _count--;
            _version++;
            return firstValue;
        }

        public bool TryRemoveLast(out T value)
        {
            if (_count == 0)
            {
                value = default;
                return false;
            }

            value = DoRemoveLast();
            return true;
        }

        public T RemoveLast()
        {
            if (_count == 0) ThrowEmpty();
            return DoRemoveLast();
        }

        private T DoRemoveLast()
        {
            T lastValue = _items[_last];
            ushort lastItemParent = _previous[_last];

            if (_last != _count - 1)
            {
                ushort movedItemParent = _previous[_count - 1];
                ushort movedItemChild = _next[_count - 1];

                // move the item at the end in place of the removed last item
                _items[_last] = _items[_count - 1];
                _previous[_last] = movedItemParent;
                _next[_last] = movedItemChild;

                // update the moved item's parent
                if (movedItemParent != Nil && movedItemParent != _last)
                {
                    _next[movedItemParent] = _last;
                }

                // update the moved item's child
                if (movedItemChild != Nil && movedItemChild != _last)
                {
                    _previous[movedItemChild] = _last;
                }

                // update first index if it is the one being moved
                if (_first == _count - 1)
                {
                    _first = _last;
                }
            }

            _items[_count - 1] = default;
            _previous[_count - 1] = default;
            _next[_count - 1] = default;

            if (_count == 1)
            {
                _first = _last = Nil;
            }
            else if (_count == 2)
            {
                _first = _last = 0;
                _previous[0] = _next[0] = Nil;
            }
            else
            {
                // if last item's parent is the one being updated then we don't need to update last
                if (lastItemParent != _count - 1)
                {
                    _last = lastItemParent;
                }
                _next[_last] = Nil;
            }

            _count--;
            _version++;
            return lastValue;
        }

        // Remove a node that is not first, nor last
        private void RemoveInternalNode(ushort removed)
        {
            Debug.Assert(_previous[removed] != Nil, "previous index when removing an internal node should not be null");
            Debug.Assert(_next[removed] != Nil, "next index when removing an internal node should not be null");

            ushort previous = _previous[removed];
            ushort next = _next[removed];

            // update previous and next nodes
            _next[previous] = next;
            _previous[next] = previous;

            // move the end item in place of the removed item
            ushort moved = (ushort)(_count - 1);
            if (removed != moved)
            {
                ushort movedItemParent = _previous[moved];
                ushort movedItemChild = _next[moved];
                _items[removed] = _items[moved];
                _items[moved] = default;
                _previous[moved] = default;
                _next[moved] = default;
                _previous[removed] = movedItemParent;
                _next[removed] = movedItemChild;

                // update moved item's parent
                if (movedItemParent != Nil)
                {
                    _next[movedItemParent] = removed;
                }

                // update moved item's child
                if (movedItemChild != Nil)
                {
                    _previous[movedItemChild] = removed;
                }

                // update first/last indices
                if (_first == moved)
                {
                    _first = removed;
                }
                if (_last == moved)
                {
                    _last = removed;
                }
            }

            _count--;
            _version++;
        }
    }
}