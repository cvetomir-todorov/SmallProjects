using SlotMachine.App.Game.Configuration;

namespace SlotMachine.App.Game.Spinning
{
    public sealed class WildcardSymbol : Symbol
    {
        public WildcardSymbol(SymbolOptions symbolOptions) : base(symbolOptions)
        {}

        public override void Evaluate(SpinContext context)
        {
            // wildcard symbols should:
            // not change current letter
            // not terminate evaluation
            // add their coefficient although it's probably 0 or very low
            context.Coefficient += SymbolOptions.Coefficient;
        }
    }
}
