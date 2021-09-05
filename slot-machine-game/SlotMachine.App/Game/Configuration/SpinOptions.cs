using FluentValidation;

namespace SlotMachine.App.Game.Configuration
{
    public sealed class SpinOptions
    {
        public int RowCount { get; set; }
        public int SymbolCount { get; set; }

        public override string ToString() =>
            $"[{nameof(RowCount)}:{RowCount}, {nameof(SymbolCount)}:{SymbolCount}]";
    }

    public sealed class SpinOptionsValidator : AbstractValidator<SpinOptions>
    {
        public SpinOptionsValidator()
        {
            RuleFor(x => x.RowCount).InclusiveBetween(@from: 1, to: 8);
            RuleFor(x => x.SymbolCount).InclusiveBetween(@from: 2, to: 8);
        }
    }
}