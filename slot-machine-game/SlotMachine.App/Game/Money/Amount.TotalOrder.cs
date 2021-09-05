using System;

namespace SlotMachine.App.Game.Money
{
    // See create file.
    public sealed partial class Amount : IEquatable<Amount>
    {
        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        public bool Equals(Amount other)
        {
            if (ReferenceEquals(other, null))
                return false;
            return this == other;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null))
                return false;
            if (ReferenceEquals(obj, this))
                return true;
            Amount other = obj as Amount;
            if (ReferenceEquals(other, null))
                return false;
            return Equals(other);
        }

        public static bool operator ==(Amount left, Amount right)
        {
            if (ReferenceEquals(left, null) && ReferenceEquals(right, null))
                return true;
            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
                return false;

            return left._value == right._value;
        }

        public static bool operator !=(Amount left, Amount right)
        {
            return !(left == right);
        }

        public static bool operator >(Amount left, Amount right)
        {
            return left._value > right._value;
        }

        public static bool operator <(Amount left, Amount right)
        {
            return left._value < right._value;
        }
    }
}
