namespace Crypto.TradeEngine.Scene;

public sealed class SymbolConfigOptions
{
    public SymbolOptions[] Symbols { get; init; } = Array.Empty<SymbolOptions>();
}

public sealed class SymbolOptions
{
    public static readonly SymbolOptions Unknown = new();

    public string Symbol { get; init; } = string.Empty;
    public string DisplayText { get; init; } = "???/???";
    public char CurrencySymbol { get; init; } = '?';
    public VolumeSpikeOptions? VolumeSpike { get; init; }

    public override string ToString()
    {
        return $"{DisplayText} {CurrencySymbol}";
    }
}

public sealed class VolumeSpikeOptions
{
    public int Length { get; init; }
    public TimeSpan Interval { get; init; }
    public decimal Ratio { get; init; }
}
