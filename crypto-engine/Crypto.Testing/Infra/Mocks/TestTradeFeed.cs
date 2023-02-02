using System.Threading.Channels;
using Crypto.TradeEngine.Scene;
using Crypto.TradeEngine.Scene.Actors;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Crypto.Testing.Infra.Mocks;

public class TestTradeFeed : ITradeFeed
{
    private readonly ILogger _logger;
    private readonly ConsoleOptions _consoleOptions;
    private readonly ChannelWriter<TradeInfo> _localFileWriter;
    private readonly ChannelWriter<TradeInfo> _backendWriter;
    private readonly ChannelWriter<TradeInfo> _volumeSpikeWriter;
    private IEnumerable<TradeInfo>? _activity;

    public TestTradeFeed(ILogger<TestTradeFeed> logger, IOptions<ConsoleOptions> consoleOptions, ChannelContainer channelContainer)
    {
        _logger = logger;
        _consoleOptions = consoleOptions.Value;
        _localFileWriter = channelContainer.LocalFile.Writer;
        _backendWriter = channelContainer.TradeBackend.Writer;
        _volumeSpikeWriter = channelContainer.VolumeSpike.Writer;
    }

    public void Dispose()
    { }

    public void SetTradeActivity(IEnumerable<TradeInfo> activity)
    {
        _activity = activity;
    }

    public async Task Start(CancellationToken ct)
    {
        if (_activity == null)
        {
            throw new InvalidOperationException($"Call {nameof(SetTradeActivity)} to provide trades for the feed.");
        }

        _logger.LogInformation("Start test trade feed");

        foreach (TradeInfo trade in _activity)
        {
            if (_consoleOptions.OutputEnabled)
            {
                _logger.LogInformation("{Trade}", trade);
            }

            await _localFileWriter.WriteAsync(trade, ct);
            await _backendWriter.WriteAsync(trade, ct);
            await _volumeSpikeWriter.WriteAsync(trade, ct);
        }
    }
}
