using System;

namespace SlotMachine.App.Game.Spinning
{
    public interface IRandomNumberGenerator
    {
        /// <summary>
        /// Generates an random integer in [0, <see cref="max"/>).
        /// </summary>
        int Generate(int max);
    }

    public sealed class SystemRandomNumberGenerator : IRandomNumberGenerator
    {
        private readonly Random _random;

        public SystemRandomNumberGenerator()
        {
            _random = new Random();
        }

        public int Generate(int max)
        {
            return _random.Next(max);
        }
    }
}
