using System;
using System.Text;

namespace SlotMachine.App.Game.Money
{
    public sealed class CreateAmountResult
    {
        public bool IsSuccess { get; set; }
        public bool ClassNotConfigured { get; set; }
        public bool ValueIsNegative { get; set; }
        public bool ValueIsGreaterThanMax { get; set; }
        public bool Overflow { get; set; }
    }

    /// <summary>
    /// Represents an amount of money:
    /// * It is immutable.
    /// * Keeps the amount value in long data type using the specified precision.
    /// * Each amount value is calculated as follows: Amount = Round(DecimalValue, Precision) * 10^Precision
    /// * Ensures amount is always within [0, MAX] range.
    /// * Avoids floating point inaccuracy in addition and subtraction operations.
    /// * For multiplication and division operations uses decimal data type and then converts back the value to long with flooring and rounding.
    /// </summary>
    public sealed partial class Amount
    {
        private readonly long _value;

        private Amount(long value)
        {
            _value = value;
        }

        public Amount Clone()
        {
            return new Amount(_value);
        }

        public static Amount Create(decimal value)
        {
            CreateAmountResult result = TryCreate(value, out Amount amount);

            if (result.ClassNotConfigured)
                throw new InvalidOperationException(
                    $"To start using the {nameof(Amount)} class first configure it using the {nameof(Configure)} static method.");
            if (result.ValueIsNegative)
                throw new ArgumentException("Value is negative.", nameof(value));
            if (result.ValueIsGreaterThanMax)
                throw new ArgumentException("Value is greater than max.", nameof(value));
            if (result.Overflow)
                throw new ArgumentException("Value caused an arithmetic overflow.", nameof(value));

            if (result.IsSuccess)
                return amount;
            else
                throw new InvalidOperationException("Unknown state of create amount result.");
        }

        public static CreateAmountResult TryCreate(decimal value, out Amount result)
        {
            result = null;

            if (ReferenceEquals(Max, null))
                return new CreateAmountResult {ClassNotConfigured = true};
            if (value < decimal.Zero)
                return new CreateAmountResult {ValueIsNegative = true};

            try
            {
                long normalizedValue = Normalize(value);
                if (normalizedValue > Max._value)
                {
                    return new CreateAmountResult {ValueIsGreaterThanMax = true};
                }

                result = new Amount(normalizedValue);
                return new CreateAmountResult {IsSuccess = true};
            }
            catch (OverflowException)
            {
                return new CreateAmountResult {Overflow = true};
            }
        }

        private static long Normalize(decimal value)
        {
            decimal rounded = Math.Round(value, decimals: Precision);
            long normalized = Convert.ToInt64(rounded * PrecisionValue);
            return normalized;
        }

        public override string ToString()
        {
            long leftFromDecPoint = _value / PrecisionValue;
            long rightFromDecPoint = _value % PrecisionValue;

            StringBuilder result = new StringBuilder(capacity: 64);
            result.Append(leftFromDecPoint);

            if (Precision > 0)
            {
                result.Append('.');

                int digitsInRight = Precision;
                long clone = rightFromDecPoint;
                while (clone > 0)
                {
                    digitsInRight--;
                    clone /= 10;
                }
                for (int i = 0; i < digitsInRight; ++i)
                {
                    result.Append('0');
                }
                if (rightFromDecPoint > 0)
                {
                    result.Append(rightFromDecPoint);
                }
            }

            return result.ToString();
        }
    }
}
