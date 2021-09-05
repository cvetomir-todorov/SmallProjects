using System;
using System.IO;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using InSync.CounterMessaging;
using InSync.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace InSync.Api.Counter
{
    /// <summary>
    /// Communicates with the counter.
    /// </summary>
    public interface ICounterClient : IDisposable
    {
        void Start();

        Task<GetStatusResult> GetStatus();

        Task<SetStatusResult> SetStatus(bool status);
    }

    public sealed class GetStatusResult
    {
        public bool IsSuccess { get; set; }
        public string IOError { get; set; }
        public bool Status { get; set; }
    }

    public sealed class SetStatusResult
    {
        public bool IsSuccess { get; set; }
        public string IOError { get; set; }
    }

    public sealed class CounterClient : ICounterClient
    {
        private readonly ILogger _logger;
        private readonly CounterOptions _options;
        private readonly SemaphoreSlim _pipeSemaphore;
        private Timer _reconnectTimer;
        private CancellationTokenSource _cts;
        private ICounterSender _counterSender;
        private int _isReconnecting;

        public CounterClient(ILogger<CounterClient> logger, IOptions<CounterOptions> options)
        {
            _logger = logger;
            _options = options.Value;
            _pipeSemaphore = new SemaphoreSlim(initialCount: 1, maxCount: 1);
        }

        public void Dispose()
        {
            _cts?.SafelyCancel();

            _cts?.Dispose();
            _reconnectTimer?.Dispose();
            _counterSender?.Dispose();
            _pipeSemaphore.Dispose();
        }

        public void Start()
        {
            _cts = new CancellationTokenSource();
            _reconnectTimer = new Timer(async state => await EnsureConnected(_cts.Token),
                state: null, dueTime: TimeSpan.Zero, _options.PipeReconnectInterval);
        }

        private async Task EnsureConnected(CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
                return;
            // make sure only 1 thread is reconnecting
            if (Interlocked.CompareExchange(ref _isReconnecting, ThreadingConstants.True, ThreadingConstants.False) == ThreadingConstants.True)
                return;

            try
            {
                await Connect(ct);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to connect to counter.");
            }
            finally
            {
                Interlocked.Exchange(ref _isReconnecting, ThreadingConstants.False);
            }
        }

        private async Task Connect(CancellationToken ct)
        {
            try
            {
                NamedPipeClientStream pipeStream = new NamedPipeClientStream(
                    serverName: ".", _options.PipeName, PipeDirection.InOut, PipeOptions.Asynchronous);

                await _pipeSemaphore.WaitAsync(ct);

                if (ct.IsCancellationRequested)
                    return;
                if (_counterSender != null && _counterSender.IsConnected)
                    return;

                _logger.LogInformation("Connecting to counter via named pipe {0}...", _options.PipeName);

                await pipeStream.ConnectAsync((int) _options.PipeConnectTimeout.TotalMilliseconds, ct);
                _counterSender = new CounterSender(pipeStream);

                _logger.LogInformation("Connected to counter.");
            }
            finally
            {
                _pipeSemaphore.Release();
            }
        }

        public async Task<GetStatusResult> GetStatus()
        {
            try
            {
                await _pipeSemaphore.WaitAsync();
                bool status = await _counterSender.GetStatus(_cts.Token);
                return new GetStatusResult {IsSuccess = true, Status = status};
            }
            catch (IOException ioException)
            {
                _logger.LogError(ioException, "Named pipe IO error.");
                return new GetStatusResult {IOError = ioException.Message};
            }
            finally
            {
                _pipeSemaphore.Release();
            }
        }

        public async Task<SetStatusResult> SetStatus(bool status)
        {
            try
            {
                await _pipeSemaphore.WaitAsync();
                await _counterSender.SetStatus(status, _cts.Token);
                return new SetStatusResult {IsSuccess = true};
            }
            catch (IOException ioException)
            {
                _logger.LogError(ioException, "Named pipe IO error.");
                return new SetStatusResult {IOError = ioException.Message};
            }
            finally
            {
                _pipeSemaphore.Release();
            }
        }
    }
}
