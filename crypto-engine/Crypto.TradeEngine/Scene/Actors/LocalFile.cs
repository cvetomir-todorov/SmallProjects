using System.Threading.Channels;
using Crypto.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Crypto.TradeEngine.Scene.Actors;

public sealed class LocalFile : IActor
{
    private readonly ILogger _logger;
    private readonly LocalFileOptions _localFileOptions;
    private readonly ChannelReader<TradeInfo> _reader;
    private StreamWriter? _localFileWriter;
    private DedicatedThreadTaskScheduler? _taskScheduler;
    private bool _disposed;

    public LocalFile(ILogger<LocalFile> logger, IOptions<LocalFileOptions> localFileOptions, ChannelContainer channelContainer)
    {
        _logger = logger;
        _localFileOptions = localFileOptions.Value;
        _reader = channelContainer.LocalFile.Reader;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            _taskScheduler?.Dispose();
            _localFileWriter?.Dispose();
            _disposed = true;
        }
    }

    public Task Start(CancellationToken ct)
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(GetType().FullName);
        }

        _localFileWriter = new StreamWriter(_localFileOptions.LocalFilePath, append: true);
        _logger.LogInformation("Local file is set to {File}", _localFileOptions.LocalFilePath);

        _taskScheduler = new DedicatedThreadTaskScheduler();
        Task.Factory.StartNew(async () => await Work(ct), ct, TaskCreationOptions.None, _taskScheduler);
        return Task.CompletedTask;
    }

    private async Task Work(CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                TradeInfo trade = await _reader.ReadAsync(ct);
                _localFileWriter!.WriteLine(trade);
                _localFileWriter.Flush();
            }
        }
        catch (OperationCanceledException)
        {
            // this means the cancellation token source has issued cancellation
        }
        catch (Exception exception)
        {
            _logger.LogCritical(exception, "Error while writing trades to a local file");
        }
    }
}
