using System;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SlotMachine.App.Game.Configuration;

namespace SlotMachine.App.Game.Spinning
{
    public sealed class SymbolGenerator
    {
        private readonly ILogger _logger;
        private readonly AllOptions _options;
        private readonly IRandomNumberGenerator _generator;

        public SymbolGenerator(ILogger<SymbolGenerator> logger, IOptions<AllOptions> options, IRandomNumberGenerator generator)
        {
            _logger = logger;
            _options = options.Value;
            _generator = generator;
        }

        public SymbolOptions ChooseSymbol()
        {
            // generates a value in [0,maxValue) so we add 1 to create a [1,100] percentage
            int percentage = _generator.Generate(max: 100) + 1;

            SymbolOptions chosenSymbol = null;

            // since symbols are a low count an O(N) algorithm is good enough
            // an O(logN) one may result in a more complex implementation which is harder to support with no actual benefits
            foreach (SymbolOptions currentSymbol in _options.Symbols)
            {
                if (percentage <= currentSymbol.Probability)
                {
                    chosenSymbol = currentSymbol;
                    break;
                }
                else
                {
                    percentage -= currentSymbol.Probability;
                }
            }

            if (chosenSymbol == null)
                throw new InvalidOperationException("Failed to generate a symbol.");

            _logger.LogDebug("Generated symbol {0}.", chosenSymbol);
            return chosenSymbol;
        }
    }
}
