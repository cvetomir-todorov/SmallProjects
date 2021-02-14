using FluentValidation;

namespace SlotMachine.App.Game.Configuration
{
    public sealed class GeneralOptions
    {
        public int MaxAmount { get; set; }

        public int AmountPrecision { get; set; }

        public int CoefficientPrecision { get; set; }

        public string Currency { get; set; }

        public override string ToString()
            => $"[{nameof(MaxAmount)}:{MaxAmount}, {nameof(AmountPrecision)}:{AmountPrecision}, {nameof(CoefficientPrecision)}:{CoefficientPrecision}, {nameof(Currency)}:{Currency}]";
    }

    public sealed class GeneralOptionsValidator : AbstractValidator<GeneralOptions>
    {
        public GeneralOptionsValidator()
        {
            RuleFor(x => x.MaxAmount).InclusiveBetween(from: 100_000, to: 10_000_000);
            RuleFor(x => x.AmountPrecision).InclusiveBetween(from: 0, to: 4);
            RuleFor(x => x.CoefficientPrecision).InclusiveBetween(from: 0, to: 4);
            RuleFor(x => x.Currency).NotEmpty().Length(exactLength: 3);
        }
    }
}
