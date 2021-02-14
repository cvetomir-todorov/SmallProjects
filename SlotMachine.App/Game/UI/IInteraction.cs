namespace SlotMachine.App.Game.UI
{
    public interface IInteraction
    {
        Result<char> TryGetChar();

        Result<decimal> TryGetDecimal();
    }

    public struct Result<T>
    {
        public Result(bool isSuccess, T value)
        {
            IsSuccess = isSuccess;
            Value = value;
        }

        public bool IsSuccess { get; set; }
        public T Value { get; set; }

        public override string ToString() => $"[{nameof(IsSuccess)}:{IsSuccess}, {nameof(Value)}:{Value}]";
    }
}