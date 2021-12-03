// ReSharper disable once CheckNamespace
namespace System.Collections.Generic
{
    public struct ArrayLinkedListNode<T>
    {
        internal ArrayLinkedListNode(T value, ushort index, int version)
        {
            Value = value;
            Index = index;
            Version = version;
        }

        public T Value { get; }
        internal ushort Index { get; }
        internal int Version { get; }
    }
}