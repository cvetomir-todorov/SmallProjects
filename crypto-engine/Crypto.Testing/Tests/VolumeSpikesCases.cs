using Crypto.Contracts;
using Crypto.TradeEngine.Scene;

namespace Crypto.Testing.Tests;

public partial class VolumeSpikes
{
    // signature: (string testName, TradeInfo[] activity, DateTime[]? nowTimeSequence, TradeMessage[] expectedSpikes)
    // testsettings.json contains the config for volume spikes against which tests are written
    private static object[] TestCases()
    {
        return new object[]
        {
            new object?[]
            {
                "nothing from binance => no spikes",
                NoActivity,
                NoNowTimeSequenceProvided,
                NoSpikesExpected
            },
            new object?[]
            {
                "0 trades in the list => no spikes",
                new[]
                {
                    Data.Trades.BnbUsdt(volume: 30, tradeTime: Initial.AddMilliseconds(-10))
                },
                NoNowTimeSequenceProvided,
                NoSpikesExpected
            },
            new object?[]
            {
                "1 trade in the list => no spike",
                new[]
                {
                    Data.Trades.BnbUsdt(volume: 30, tradeTime: Initial.AddMilliseconds(-20)),
                    Data.Trades.BnbUsdt(volume: 40, tradeTime: Initial.AddMilliseconds(-10))
                },
                NoNowTimeSequenceProvided,
                NoSpikesExpected
            },
            new object?[]
            {
                "1 trade in the list => spike",
                new[]
                {
                    Data.Trades.BnbUsdt(volume: 30, tradeTime: Initial.AddMilliseconds(-20)),
                    Data.Trades.BnbUsdt(volume: 1000, tradeTime: Initial.AddMilliseconds(-10))
                },
                NoNowTimeSequenceProvided,
                new[]
                {
                    Data.TradeMessages.BnbUsdt(volume: 1000, Initial.AddMilliseconds(-10))
                }
            },
            new object?[]
            {
                "full list => no spike",
                new[]
                {
                    Data.Trades.BnbUsdt(volume: 50, tradeTime: Initial.AddMilliseconds(-50)),
                    Data.Trades.BnbUsdt(volume: 100, tradeTime: Initial.AddMilliseconds(-40)),
                    Data.Trades.BnbUsdt(volume: 75, tradeTime: Initial.AddMilliseconds(-30)),
                    Data.Trades.BnbUsdt(volume: 120, tradeTime: Initial.AddMilliseconds(-20)),
                    Data.Trades.BnbUsdt(volume: 60, tradeTime: Initial.AddMilliseconds(-10))
                },
                NoNowTimeSequenceProvided,
                NoSpikesExpected
            },
            new object?[]
            {
                "full list => 2 spikes",
                new[]
                {
                    Data.Trades.BnbUsdt(volume: 50, tradeTime: Initial.AddMilliseconds(-50)),
                    Data.Trades.BnbUsdt(volume: 50, tradeTime: Initial.AddMilliseconds(-40)),
                    Data.Trades.BnbUsdt(volume: 50, tradeTime: Initial.AddMilliseconds(-30)),
                    Data.Trades.BnbUsdt(volume: 251, tradeTime: Initial.AddMilliseconds(-20)),
                    Data.Trades.BnbUsdt(volume: 1250, tradeTime: Initial.AddMilliseconds(-10))
                },
                NoNowTimeSequenceProvided,
                new[]
                {
                    Data.TradeMessages.BnbUsdt(volume: 251, tradeTime: Initial.AddMilliseconds(-20)),
                    Data.TradeMessages.BnbUsdt(volume: 1250, tradeTime: Initial.AddMilliseconds(-10))
                }
            },
            new object?[]
            {
                "different symbols are processed separately",
                new[]
                {
                    Data.Trades.BnbUsdt(volume: 1000, tradeTime: Initial),
                    Data.Trades.AdaUsdt(volume: 50, tradeTime: Initial),
                    Data.Trades.BnbUsdt(volume: 1000, tradeTime: Initial),
                    Data.Trades.AdaUsdt(volume: 50, tradeTime: Initial),
                    Data.Trades.BnbUsdt(volume: 1000, tradeTime: Initial),
                    Data.Trades.AdaUsdt(volume: 50, tradeTime: Initial),
                    Data.Trades.BnbUsdt(volume: 1000, tradeTime: Initial),
                    Data.Trades.AdaUsdt(volume: 50, tradeTime: Initial),
                    Data.Trades.BnbUsdt(volume: 1000, tradeTime: Initial),
                    Data.Trades.AdaUsdt(volume: 50, tradeTime: Initial),
                    Data.Trades.BnbUsdt(volume: 1000, tradeTime: Initial),
                    Data.Trades.AdaUsdt(volume: 50, tradeTime: Initial)
                },
                NoNowTimeSequenceProvided,
                NoSpikesExpected
            },
            new object?[]
            {
                "all old trades are purged from list",
                new[]
                {
                    Data.Trades.BnbUsdt(volume: 1000, tradeTime: Initial.AddMilliseconds(-1200)),
                    Data.Trades.BnbUsdt(volume: 1000, tradeTime: Initial.AddMilliseconds(-1100)),
                    Data.Trades.BnbUsdt(volume: 1, tradeTime: Initial),
                    Data.Trades.BnbUsdt(volume: 10, tradeTime: Initial.AddMilliseconds(1100))
                },
                new[]
                {
                    Initial.AddMilliseconds(-1100), // 100ms after 1st
                    Initial.AddMilliseconds(-1000), // 100ms after 2nd, 200ms after 1st
                    Initial.AddMilliseconds(100), // 100ms after 3rd, 1200ms after 2nd, 1300ms after 1st
                    Initial.AddMilliseconds(1200) // 100ms after 4th, 1200ms after 3rd, 2300ms after 2nd, 3400ms after 1st
                    // 1st and 2nd need to be removed which would make 4th a spike compared to the 3rd - 10*1 > 1*5
                },
                new[]
                {
                    Data.TradeMessages.BnbUsdt(volume: 10, tradeTime: Initial.AddMilliseconds(1100))
                }
            }
        };
    }

    private static readonly TradeInfo[] NoActivity = Array.Empty<TradeInfo>();
    private static readonly DateTime[]? NoNowTimeSequenceProvided = null;
    private static readonly TradeMessage[] NoSpikesExpected = Array.Empty<TradeMessage>();
}
