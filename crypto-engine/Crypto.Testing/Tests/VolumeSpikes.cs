using Crypto.Contracts;
using Crypto.Testing.Infra;
using Crypto.Testing.Infra.Mocks;
using Crypto.TradeEngine.Scene;
using Crypto.TradeEngine.Scene.Actors;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace Crypto.Testing.Tests;

public partial class VolumeSpikes
{
    private static readonly DateTime Initial = new(year: 2020, month: 1, day: 1, hour: 1, minute: 0, second: 10, millisecond: 0, DateTimeKind.Utc);
    private readonly Instrumentation _instru;

    public VolumeSpikes()
    {
        _instru = new Instrumentation();
    }

    [SetUp]
    public void BeforeEachTestCase()
    {
        int count = _instru.DrainConsumer.DrainVolumeSpikes(TimeSpan.FromSeconds(1));
        TestContext.Out.WriteLine("Drained {0} volume spikes before test case", count);
    }

    [TestCaseSource(nameof(TestCases))]
    public async Task All(string testName, TradeInfo[] activity, DateTime[]? nowTimeSequence, TradeMessage[] expectedSpikes)
    {
        // given
        IServiceProvider serviceProvider = new TestStartup(_instru.Configuration).ConfigureServices();
        TestPlay play = (TestPlay)serviceProvider.GetRequiredService<IPlay>();
        TestTradeFeed tradeFeed = (TestTradeFeed)serviceProvider.GetRequiredService<ITradeFeed>();
        TestDateTime dateTime = (TestDateTime)serviceProvider.GetRequiredService<IDateTime>();

        tradeFeed.SetTradeActivity(activity);
        play.SetDelayBeforeStop(TimeSpan.FromSeconds(1));
        if (nowTimeSequence == null)
        {
            dateTime.SetInitial(Initial);
        }
        else
        {
            dateTime.SetNowSequence(new Queue<DateTime>(nowTimeSequence));
        }
        TradeConsumerContext consumerContext = _instru.TradeConsumer.StartCollectingVolumeSpikes();

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
        List<TradeMessage> actualSpikes = consumerContext.TradeMessages;
        actualSpikes.Should().BeEquivalentTo(expectedSpikes);
    }
}
