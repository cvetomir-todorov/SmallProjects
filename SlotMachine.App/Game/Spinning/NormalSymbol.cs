using SlotMachine.App.Game.Configuration;

namespace SlotMachine.App.Game.Spinning
{
    public sealed class NormalSymbol : Symbol
    {
        public NormalSymbol(SymbolOptions symbolOptions) : base(symbolOptions)
        {}

        public override void Evaluate(SpinContext context)
        {
            if (context.SymbolLetter.Length > 0)
            {
                if (context.SymbolLetter == SymbolOptions.Letter)
                {
                    context.Coefficient += SymbolOptions.Coefficient;
                }
                else
                {
                    context.Coefficient = 0;
                    context.Continue = false;
                }
            }
            else
            {
                context.SymbolLetter = SymbolOptions.Letter;
                context.Coefficient += SymbolOptions.Coefficient;
            }
        }
    }
}
