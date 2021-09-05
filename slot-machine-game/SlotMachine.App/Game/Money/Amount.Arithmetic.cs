using System;

namespace SlotMachine.App.Game.Money
{
    // See create file.
    public sealed partial class Amount
    {
        public static Amount operator +(Amount left, Amount right)
        {
            if (ReferenceEquals(left, null))
                throw new ArgumentNullException(nameof(left));
            if (ReferenceEquals(right, null))
                throw new ArgumentNullException(nameof(right));

            checked
            {
                long value = left._value + right._value;
                if (value > Max._value)
                    throw new InvalidOperationException("Cannot allow addition result to be more than max amount.");

                return new Amount(value);
            }
        }

        public static Amount operator -(Amount left, Amount right)
        {
            if (ReferenceEquals(left, null))
                throw new ArgumentNullException(nameof(left));
            if (ReferenceEquals(right, null))
                throw new ArgumentNullException(nameof(right));

            checked
            {
                long result = left._value - right._value;
                if (result < 0)
                    throw new InvalidOperationException("Cannot subtract from lower value.");

                return new Amount(result);
            }
        }

        public static Amount operator /(Amount left, decimal right)
        {
            if (ReferenceEquals(left, null))
                throw new ArgumentNullException(nameof(left));
            if (right <= decimal.Zero)
                throw new ArgumentException("Cannot divide by negative value or zero.", nameof(right));

            decimal divisionResult = left._value / right;
            decimal flooredValue = Math.Floor(divisionResult);
            long value = Convert.ToInt64(flooredValue);
            if (value > Max._value)
                throw new InvalidOperationException("Cannot allow division result to be to more than max amount.");

            return new Amount(value);
        }

        public static Amount operator *(Amount left, decimal right)
        {
            if (ReferenceEquals(left, null))
                throw new ArgumentNullException(nameof(left));
            if (right < decimal.Zero)
                throw new ArgumentException("Cannot multiply by negative value.", nameof(right));

            decimal multiplicationResult = left._value * right;
            decimal roundedValue = Math.Round(multiplicationResult, decimals: Precision);
            long value = Convert.ToInt64(roundedValue);
            if (value > Max._value)
                throw new InvalidOperationException("Cannot allow multiplication result to be to more than max amount.");

            return new Amount(value);
        }
    }
}
