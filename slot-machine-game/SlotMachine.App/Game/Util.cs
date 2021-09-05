namespace SlotMachine.App.Game
{
    internal static class Util
    {
        public static int CalculatePrecisionValue(int precision)
        {
            int precisionValue = 1;

            for (int i = 0; i < precision; ++i)
            {
                precisionValue *= 10;
            }

            return precisionValue;
        }
    }
}
