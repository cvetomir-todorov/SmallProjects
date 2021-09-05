using System;

namespace SlotMachine.App.Game.UI
{
    public sealed class ConsoleInteraction : IInteraction
    {
        public Result<char> TryGetChar()
        {
            string line = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(line))
                return new Result<char>();
            else
                return new Result<char>(isSuccess: true, value: line[0]);
        }

        public Result<decimal> TryGetDecimal()
        {
            string line = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(line))
                return new Result<decimal>();
            else if (!decimal.TryParse(line, out decimal value))
                return new Result<decimal>();
            else
                return new Result<decimal>(isSuccess: true, value: value);
        }
    }
}
