using System;

namespace SlotMachine.App.Game.Money
{
    // See create file.
    public sealed partial class Amount
    {
        static Amount()
        {
            Zero = new Amount(0);
        }

        private static int Precision { get; set; }
        private static int PrecisionValue { get; set; }
        public static Amount Zero { get; private set; }
        public static Amount Max { get; private set; }

        public static void Configure(decimal maxAmount, int precision)
        {
            if (maxAmount <= decimal.Zero)
                throw new ArgumentException("Max amount should be > 0.", nameof(maxAmount));
            if (precision < 0 || precision > 4)
                throw new ArgumentException("Precision should be in [0,4].", nameof(precision));

            try
            {
                Precision = precision;
                PrecisionValue = Util.CalculatePrecisionValue(precision);

                long normalized = Normalize(maxAmount);
                if (normalized == 0)
                {
                    throw new InvalidOperationException("Value is 0 after rounding it using the specified precision.");
                }
                Max = new Amount(normalized);
            }
            catch (OverflowException overflowException)
            {
                throw new InvalidOperationException("Max amount value leads to overflow.", overflowException);
            }
        }
    }
}
