namespace Crypto.Testing.Tests;

public partial class Trades
{
    // signature: (string testName, TradeInfo[] activity)
    private static readonly object[] TestCases =
    {
        new object[]
        {
            "single trade",
            new[]
            {
                Data.Trades.BnbUsdt(volume: 30)
            }
        },
        new object[]
        {
            "multiple trades",
            new[]
            {
                Data.Trades.BnbUsdt(volume: 30),
                Data.Trades.AdaUsdt(volume: 500),
                Data.Trades.BnbUsdt(volume: 150),
                Data.Trades.AdaUsdt(volume: 60),
                Data.Trades.BnbUsdt(volume: 1200),
                Data.Trades.AdaUsdt(volume: 4)
            }
        }
    };
}
