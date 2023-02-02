using Crypto.Testing.Infra;
using NUnit.Framework;
using Serilog;
using Serilog.Events;

namespace Crypto.Testing;

[SetUpFixture]
public static class Program
{
    [OneTimeSetUp]
    public static void Begin()
    {
        SetupSerilog();
        DrainRabbitMq();
    }

    private static void SetupSerilog()
    {
        LoggerConfiguration logging = new LoggerConfiguration()
            .MinimumLevel.Is(LogEventLevel.Verbose)
            .Enrich.FromLogContext()
            .WriteTo.Console();
        Log.Logger = logging.CreateLogger();
    }

    private static void DrainRabbitMq()
    {
        Instrumentation instru = new();

        TestContext.Out.WriteLine("Draining RabbitMQ before all tests...");
        int tradeCount = instru.DrainConsumer.DrainTrades(TimeSpan.FromSeconds(1));
        int volumeSpikeCount = instru.DrainConsumer.DrainVolumeSpikes(TimeSpan.FromSeconds(1));
        TestContext.Out.WriteLine("Drained {0} trades and {1} volume spikes before tests", tradeCount, volumeSpikeCount);
    }
}
