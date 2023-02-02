using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Crypto.TradeEngine.Scene;

public interface IPlay : IDisposable
{
    Task Start();
}

public class Play : IPlay
{
    private readonly ILogger _logger;
    private readonly ConsoleOptions _consoleOptions;
    private readonly IEnumerable<IActor> _actors;
    private readonly CancellationTokenSource _cts;
    private bool _disposed;

    public Play(ILogger<Play> logger, IOptions<ConsoleOptions> consoleOptions, IEnumerable<IActor> actors)
    {
        _logger = logger;
        _consoleOptions = consoleOptions.Value;
        _actors = actors;
        _cts = new CancellationTokenSource();
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(obj: this);
            _disposed = true;
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _cts.Dispose();

            foreach (IActor actor in _actors)
            {
                actor.Dispose();
            }
        }
    }

    public async Task Start()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(GetType().FullName);
        }

        foreach (IActor actor in _actors)
        {
            await actor.Start(_cts.Token);
        }

        _logger.LogInformation("Console output is {Status}", _consoleOptions.OutputEnabled ? "ENABLED" : "DISABLED");
        _logger.LogInformation("All actors started and the play beings. Press ENTER to stop");
        WaitForStop();
        _cts.Cancel();
    }

    protected virtual void WaitForStop()
    {
        Console.ReadLine();
    }
}
