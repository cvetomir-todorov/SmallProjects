using System.Threading.Channels;
using Crypto.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Crypto.TradeEngine.Scene.Actors;

public sealed class VolumeSpikeDetector : IActor
{
    private readonly ILogger _logger;
    private readonly ConsoleOptions _consoleOptions;
    private readonly IDateTime _dateTime;
    private readonly ChannelReader<TradeInfo> _reader;
    private readonly ChannelWriter<TradeInfo> _backendWriter;
    private readonly Dictionary<string, VolumeContext> _volumeMap;
    private DedicatedThreadTaskScheduler? _taskScheduler;
    private bool _disposed;

    public VolumeSpikeDetector(
        ILogger<VolumeSpikeDetector> logger,
        IOptions<ConsoleOptions> consoleOptions,
        IOptions<SymbolConfigOptions> symbolConfigOptions,
        IDateTime dateTime,
        ChannelContainer channelContainer)
    {
        _logger = logger;
        _consoleOptions = consoleOptions.Value;
        _dateTime = dateTime;
        _reader = channelContainer.VolumeSpike.Reader;
        _backendWriter = channelContainer.VolumeSpikeBackend.Writer;
        _volumeMap = new Dictionary<string, VolumeContext>();

        foreach (SymbolOptions symbolOptions in symbolConfigOptions.Value.Symbols)
        {
            if (symbolOptions.VolumeSpike == null)
            {
                continue;
            }

            _volumeMap.Add(symbolOptions.Symbol, new VolumeContext(symbolOptions.VolumeSpike));
        }
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _taskScheduler?.Dispose();
            _disposed = true;
        }
    }

    public Task Start(CancellationToken ct)
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(GetType().FullName);
        }

        _logger.LogInformation("Start detection of volume spikes");

        _taskScheduler = new DedicatedThreadTaskScheduler();
        Task.Factory.StartNew(() => Work(ct), ct, TaskCreationOptions.None, _taskScheduler);
        return Task.CompletedTask;
    }

    private async Task Work(CancellationToken ct)
    {
        try
        {
            await DoWork(ct);
        }
        catch (OperationCanceledException)
        {
            // this means the cancellation token source has issued cancellation
        }
        catch (Exception exception)
        {
            _logger.LogCritical(exception, "Error while detecting volume spikes");
        }
    }

    private async Task DoWork(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            TradeInfo trade = await _reader.ReadAsync(ct);
            if (!_volumeMap.TryGetValue(trade.Symbol, out VolumeContext? context))
            {
                continue;
            }

            if (DetectSpike(context, trade))
            {
                if (_consoleOptions.OutputEnabled)
                {
                    _logger.LogWarning("{Trade} Volume spike!", trade);
                }
                if (!_backendWriter.TryWrite(trade))
                {
                    _logger.LogError("Failed to send volume spike to backend");
                }
            }
        }
    }

    /// <summary>
    /// Volume spike is detected in the following way:
    /// * we keep the last K trades for the last M seconds
    /// * when a new trade comes in, if its volume is N times higher than the average of the trades in the list, it is considered a spike
    /// * if the list has 0 trades then the new trade is never a spike
    /// * if the list has at least 1 trade then the new trade could be a spike
    /// * we add the new trade in the list
    /// * we purge trades not within the last M seconds
    /// * we purge trades when the list has more than K trades
    ///
    /// To calculate the average in order to avoid decimal division we simply keep a sum of the trades' volumes and compare [new-volume * list-count] to [sum * ratio]
    /// We keep trades ordered by trade time to minimize efforts for purging old trades
    /// </summary>
    // TODO: periodically re-calculate the sum in order to avoid loss of fraction
    private bool DetectSpike(VolumeContext context, TradeInfo trade)
    {
        bool isSpike = false;

        // purge old
        DateTime threshold = _dateTime.UtcNow().Add(-context.Config.Interval);
        while (context.List.Count > 0 && context.List.First!.Value.TradeTime < threshold)
        {
            TradeInfo oldTrade = context.List.First!.Value;
            context.List.RemoveFirst();
            context.Sum -= oldTrade.Volume;
        }

        // detect spike
        if (context.List.Count > 0)
        {
            // despite using decimals, check for overflow, just in case
            checked
            {
                if (trade.Volume * context.List.Count > context.Sum * context.Config.Ratio)
                {
                    isSpike = true;
                }
            }
        }

        // add new trade
        context.Sum += trade.Volume;
        context.List.AddLast(trade);

        // purge excess
        if (context.List.Count > context.Config.Length)
        {
            TradeInfo oldest = context.List.First!.Value;
            context.List.RemoveFirst();
            context.Sum -= oldest.Volume;
        }

        return isSpike;
    }

    private sealed class VolumeContext
    {
        public VolumeContext(VolumeSpikeOptions config)
        {
            Config = config;
            List = new LinkedList<TradeInfo>();
            Sum = 0.0m;
        }

        public VolumeSpikeOptions Config { get; }
        // TODO: use a circular linked list implemented via an array in order to avoid unnecessary allocations
        public LinkedList<TradeInfo> List { get; }
        public decimal Sum { get; set; }
    }
}
