namespace SlotMachine.App.Game.Spinning
{
    public sealed class SpinContext
    {
        public SpinContext()
        {
            Clear();
        }

        public void Clear()
        {
            SymbolLetter = string.Empty;
            Coefficient = 0;
            Continue = true;
        }

        public string SymbolLetter { get; set; }

        public int Coefficient { get; set; }

        public bool Continue { get; set; }

        public override string ToString()
            => $"[{nameof(SymbolLetter)}:{SymbolLetter}, {nameof(Coefficient)}:{Coefficient}, {nameof(Continue)}:{Continue}]";
    }
}
