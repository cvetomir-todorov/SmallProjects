namespace Crypto.TradeEngine.Scene;

public sealed class TradeInfo
{
    public DateTime TradeTime { get; init; }
    public string Symbol { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public decimal Quantity { get; init; }
    public decimal Volume { get; init; }
    public SymbolOptions SymbolConfig { get; init; } = SymbolOptions.Unknown;

    public override string ToString()
    {
        return $"{TradeTime:hh:mm:ss.fff} {SymbolConfig.DisplayText,10} {Price,10:0.0##}{SymbolConfig.CurrencySymbol} {Quantity,10:0.0##} {Volume,10:0.0##}";
    }
}
