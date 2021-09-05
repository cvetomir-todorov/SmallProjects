using FluentValidation;

namespace SlotMachine.App.Game.Configuration
{
    public sealed class SymbolOptions
    {
        public SymbolType Type { get; set; }
        public string Name { get; set; }
        public string Letter { get; set; }
        public int Coefficient { get; set; }
        public int Probability { get; set; }

        public override string ToString() =>
            $"[{Name}({Letter}), {Type}, {nameof(Coefficient)}:{Coefficient}, {nameof(Probability)}:{Probability}]";
    }

    public enum SymbolType
    {
        Wildcard, Normal
    }

    public sealed class SymbolOptionsValidator : AbstractValidator<SymbolOptions>
    {
        public SymbolOptionsValidator(int coefficientPrecision)
        {
            int coefficientPrecisionValue = Util.CalculatePrecisionValue(coefficientPrecision);

            RuleFor(x => x.Name).NotEmpty().Length(min: 2, max: 16);
            RuleFor(x => x.Letter).NotNull().Length(exactLength: 1);
            RuleFor(x => x.Coefficient)
                .Must((symbol, coefficient) =>
                {
                    if (symbol.Type != SymbolType.Wildcard)
                        return true;
                    return coefficient == 0;
                })
                .WithMessage("Wildcard symbol * should have coefficient value of 0.")
                .Must((symbol, coefficient) =>
                {
                    if (symbol.Type != SymbolType.Normal)
                        return true;
                    return coefficient > 0 && coefficient <= 10 * coefficientPrecisionValue;
                })
                .WithMessage($"Symbols should have a coefficient in [1,{10 * coefficientPrecisionValue}].");
            RuleFor(x => x.Probability).InclusiveBetween(@from: 1, to: 99);
        }
    }
}