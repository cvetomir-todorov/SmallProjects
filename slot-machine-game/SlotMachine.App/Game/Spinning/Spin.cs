using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SlotMachine.App.Game.Configuration;
using SlotMachine.App.Game.UI;

namespace SlotMachine.App.Game.Spinning
{
    public interface ISpin
    {
        /// <summary>
        /// Spins and calculates a coefficient indicating profit.
        /// Resulting coefficient is in [0, RowCount * SymbolCount * HighestCoefficient].
        /// </summary>
        SpinResult Execute();
    }

    public sealed class SpinResult
    {
        public int Coefficient { get; set; }
    }

    public interface ISpinConfiguration
    {
        void PrepareSymbols();
    }

    public sealed class Spin : ISpin, ISpinConfiguration
    {
        private readonly ILogger _logger;
        private readonly AllOptions _options;
        private readonly SymbolGenerator _symbolGenerator;
        private readonly IOutput _output;
        private readonly Dictionary<string, Symbol> _symbols;

        public Spin(ILogger<Spin> logger, IOptions<AllOptions> options, SymbolGenerator symbolGenerator, IOutput output)
        {
            _logger = logger;
            _options = options.Value;
            _symbolGenerator = symbolGenerator;
            _output = output;
            _symbols = new Dictionary<string, Symbol>();
        }

        public void PrepareSymbols()
        {
            _logger.LogInformation("Preparing symbols...");
            foreach (SymbolOptions symbolOptions in _options.Symbols)
            {
                switch (symbolOptions.Type)
                {
                    case SymbolType.Normal:
                        _symbols.Add(symbolOptions.Letter, new NormalSymbol(symbolOptions));
                        break;
                    case SymbolType.Wildcard:
                        _symbols.Add(symbolOptions.Letter, new WildcardSymbol(symbolOptions));
                        break;
                    default:
                        throw new NotSupportedException($"{typeof(SymbolType).FullName} value {symbolOptions.Type} is not supported.");
                }
            }
            _logger.LogInformation("Prepared symbols.");
        }

        public SpinResult Execute()
        {
            Symbol[] symbols = new Symbol[_options.Spin.SymbolCount];
            StringBuilder rowBuilder = new StringBuilder();
            SpinContext context = new SpinContext();
            int coefficient = 0;

            for (int rowIndex = 0; rowIndex < _options.Spin.RowCount; ++rowIndex)
            {
                GenerateSymbolRow(rowBuilder, symbols);
                EvaluateSymbolRow(context, symbols);
                coefficient += context.Coefficient;
                OutputSymbolRow(rowBuilder.ToString(), context);
            }

            return new SpinResult {Coefficient = coefficient};
        }

        private void GenerateSymbolRow(StringBuilder rowBuilder, Symbol[] symbols)
        {
            rowBuilder.Clear();

            for (int symbolIndex = 0; symbolIndex < _options.Spin.SymbolCount; ++symbolIndex)
            {
                _output.AddSuspense();
                SymbolOptions chosenSymbol = _symbolGenerator.ChooseSymbol();
                symbols[symbolIndex] = _symbols[chosenSymbol.Letter];
                rowBuilder.Append(chosenSymbol.Letter).Append(' ');
            }

            _logger.LogDebug("Generated row {0}", rowBuilder.ToString());
        }

        private void EvaluateSymbolRow(SpinContext context, Symbol[] symbols)
        {
            context.Clear();

            foreach (Symbol symbol in symbols)
            {
                symbol.Evaluate(context);
                if (!context.Continue)
                {
                    break;
                }
            }

            _logger.LogDebug("Evaluated row {0}.", context);
        }

        private void OutputSymbolRow(string rowOutput, SpinContext context)
        {
            if (context.Coefficient > 0)
            {
                _output.Congratulate(rowOutput);
            }
            else
            {
                _output.Inform(rowOutput);
            }
        }
    }
}
