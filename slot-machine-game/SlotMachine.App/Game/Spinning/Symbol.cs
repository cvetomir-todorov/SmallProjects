using SlotMachine.App.Game.Configuration;

namespace SlotMachine.App.Game.Spinning
{
    public abstract class Symbol
    {
        private readonly SymbolOptions _symbolOptions;

        protected Symbol(SymbolOptions symbolOptions)
        {
            _symbolOptions = symbolOptions;
        }

        protected SymbolOptions SymbolOptions => _symbolOptions;

        public abstract void Evaluate(SpinContext context);

        public override string ToString() => _symbolOptions.ToString();
    }
}
