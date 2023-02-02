using Crypto.Contracts;
using Crypto.TradeEngine.Scene;
using Google.Protobuf.WellKnownTypes;

namespace Crypto.Testing.Tests;

public static class Data
{
    public static class Symbols
    {
        public static readonly SymbolOptions BnbUsdt = new() { Symbol = "BNBUSDT", DisplayText = "BNB/USDT", CurrencySymbol = '$' };
        public static readonly SymbolOptions AdaUsdt = new() { Symbol = "ADAUSDT", DisplayText = "ADA/USDT", CurrencySymbol = '$' };
    }

    public static class Prices
    {
        public static readonly decimal BnbUsdt = 305.0m;
        public static readonly decimal AdaUsdt = 0.405m;
    }

    public static class Trades
    {
        public static TradeInfo BnbUsdt(decimal volume, DateTime? tradeTime = null)
        {
            return ForSymbol(Symbols.BnbUsdt, Prices.BnbUsdt, volume, tradeTime);
        }

        public static TradeInfo AdaUsdt(decimal volume, DateTime? tradeTime = null)
        {
            return ForSymbol(Symbols.AdaUsdt, Prices.AdaUsdt, volume, tradeTime);
        }

        private static TradeInfo ForSymbol(SymbolOptions symbol, decimal price, decimal volume, DateTime? tradeTime = null)
        {
            if (tradeTime == null)
            {
                tradeTime = DateTime.UtcNow;
            }

            return new TradeInfo
            {
                Symbol = symbol.Symbol,
                SymbolConfig = symbol,
                TradeTime = tradeTime.Value,
                Price = price,
                Quantity = volume/price,
                Volume = volume
            };
        }
    }

    public static class TradeMessages
    {
        public static TradeMessage BnbUsdt(decimal volume, DateTime tradeTime)
        {
            return ForSymbol(Symbols.BnbUsdt.Symbol, Prices.BnbUsdt, volume, tradeTime);
        }

        public static TradeMessage AdaUsdt(decimal volume, DateTime tradeTime)
        {
            return ForSymbol(Symbols.AdaUsdt.Symbol, Prices.AdaUsdt, volume, tradeTime);
        }

        private static TradeMessage ForSymbol(string symbol, decimal price, decimal volume, DateTime tradeTime)
        {
            return new TradeMessage
            {
                Symbol = symbol,
                TradeTime = tradeTime.ToTimestamp(),
                Price = Convert.ToDouble(price),
                Quantity = Convert.ToDouble(volume/price),
                Volume = Convert.ToDouble(volume)
            };
        }
    }
}
