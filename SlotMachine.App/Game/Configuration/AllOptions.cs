using System.Collections.Generic;
using System.Linq;
using FluentValidation;

namespace SlotMachine.App.Game.Configuration
{
    public sealed class AllOptions
    {
        public GeneralOptions General { get; set; }
        public SpinOptions Spin { get; set; }
        public List<SymbolOptions> Symbols { get; set; } = new List<SymbolOptions>();

        public override string ToString() =>
            $"[{nameof(General)}:{General}, {nameof(Spin)}:{Spin}, {nameof(Symbols)} count:{Symbols.Count}]";
    }

    public sealed class AllOptionsValidator : AbstractValidator<AllOptions>
    {
        public AllOptionsValidator()
        {
            RuleFor(x => x.General).NotNull().SetValidator(new GeneralOptionsValidator());
            RuleFor(x => x.Spin).NotNull().SetValidator(new SpinOptionsValidator());
            RuleFor(x => x.Symbols).NotEmpty();
            RuleForEach(x => x.Symbols).SetValidator(options => new SymbolOptionsValidator(options.General.CoefficientPrecision));
            RuleFor(x => x.Symbols)
                .Must(symbols =>
                {
                    if (symbols == null)
                        return true;
                    int probabilitySum = symbols.Select(s => s.Probability).Sum();
                    return probabilitySum == 100;
                })
                .WithMessage("Sum of symbol probabilities should be exactly 100.")
                .Must(symbols =>
                {
                    if (symbols == null)
                        return true;
                    bool containsWildcard = null != symbols.FirstOrDefault(symbol => symbol.Type == SymbolType.Wildcard);
                    return containsWildcard;
                })
                .WithMessage("Symbols should contain at least one wildcard symbol.")
                .Must(symbols =>
                {
                    if (symbols == null)
                        return true;
                    HashSet<string> uniqueLetters = new HashSet<string>();
                    bool areUnique = symbols.All(s => uniqueLetters.Add(s.Letter));
                    return areUnique;
                })
                .WithMessage("Symbol letters should be unique.");
            RuleFor(x => x)
                .Must(options =>
                {
                    int greatestSymbolCoefficient = options.Symbols.Select(s => s.Coefficient).Max();
                    int maxCombinedCoefficient = options.Spin.RowCount * options.Spin.SymbolCount * greatestSymbolCoefficient;
                    int coefficientPrecisionValue = Util.CalculatePrecisionValue(options.General.CoefficientPrecision);
                    return maxCombinedCoefficient > coefficientPrecisionValue;
                })
                .WithMessage("Max possible profit for a spin needs to be greater than the stake amount.");
        }
    }
}
