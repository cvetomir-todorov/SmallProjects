using Crypto.Contracts;
using Crypto.Testing.Infra;
using Crypto.Testing.Infra.Mocks;
using Crypto.TradeEngine.Scene;
using Crypto.TradeEngine.Scene.Actors;
using FluentAssertions;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Crypto.Testing.Tests;

public partial class Trades
{
    private readonly Instrumentation _instru;

    public Trades()
    {
        _instru = new Instrumentation();
    }

    [SetUp]
    public void BeforeEachTestCase()
    {
        int count = _instru.DrainConsumer.DrainTrades(TimeSpan.FromSeconds(1));
        TestContext.Out.WriteLine("Drained {0} trades before test case", count);
    }

    [TestCaseSource(nameof(TestCases))]
    public async Task All(string testName, TradeInfo[] activity)
    {
        // given
        IServiceProvider serviceProvider = new TestStartup(_instru.Configuration).ConfigureServices();
        TestPlay play = (TestPlay)serviceProvider.GetRequiredService<IPlay>();
        TestTradeFeed tradeFeed = (TestTradeFeed)serviceProvider.GetRequiredService<ITradeFeed>();

        tradeFeed.SetTradeActivity(activity);
        play.SetDelayBeforeStop(TimeSpan.FromSeconds(1));
        TradeConsumerContext consumerContext = _instru.TradeConsumer.StartCollectingTrades();

        // when
        try
        {
            await play.Start();
        }
        finally
        {
            play.Dispose();
            consumerContext.Dispose();
        }

        // then
        List<TradeMessage> expected = activity
            .Select(trade => new TradeMessage
            {
                TradeTime = trade.TradeTime.ToTimestamp(),
                Symbol = trade.Symbol,
                Price = Convert.ToDouble(trade.Price),
                Quantity = Convert.ToDouble(trade.Quantity),
                Volume = Convert.ToDouble(trade.Volume)
            })
            .ToList();

        List<TradeMessage> actual = consumerContext.TradeMessages;
        actual.Should().BeEquivalentTo(expected);
    }
}
