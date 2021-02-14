using System;
using System.IO.Pipes;
using System.Threading;
using System.Threading.Tasks;
using InSync.Counter.SensorData;
using InSync.CounterMessaging;
using InSync.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace InSync.Counter.Control
{
    /// <summary>
    /// Processes control commands for the counter such as start/stop.
    /// </summary>
    public interface IControlReceiver : IDisposable
    {
        void Start(string @interface, int port);
    }

    public sealed class ControlReceiver : IControlReceiver
    {
        private readonly ILogger _logger;
        private readonly ISensorDataReceiver _sensorDataReceiver;
        private readonly ControlOptions _options;
        private CancellationTokenSource _listenCts;
        private CancellationTokenSource _processCts;
        private NamedPipeServerStream _pipeStream;
        private ICounterReceiver _counterReceiver;
        private string _interface;
        private int _port;

        public ControlReceiver(
            ILogger<ControlReceiver> logger,
            ISensorDataReceiver sensorDataReceiver,
            IOptions<ControlOptions> options)
        {
            _logger = logger;
            _sensorDataReceiver = sensorDataReceiver;
            _options = options.Value;
        }

        public void Dispose()
        {
            _processCts.SafelyCancel();
            _listenCts.SafelyCancel();

            _processCts.Dispose();
            _listenCts?.Dispose();
            _counterReceiver?.Dispose();
        }

        public void Start(string @interface, int port)
        {
            _interface = @interface;
            _port = port;
            _listenCts = new CancellationTokenSource();

            Task _ = Task.Run(async () => await ListenToControlMessages());
        }

        private async Task ListenToControlMessages()
        {
            while (!_listenCts.IsCancellationRequested)
            {
                try
                {
                    _processCts?.SafelyCancel();
                    _processCts?.Dispose();
                    _pipeStream?.Dispose();
                    _counterReceiver?.Dispose();

                    _processCts = new CancellationTokenSource();
                    _pipeStream = new NamedPipeServerStream(
                        _options.PipeName, PipeDirection.InOut, maxNumberOfServerInstances: 1, PipeTransmissionMode.Message, PipeOptions.Asynchronous);
                    _counterReceiver = new CounterReceiver(_pipeStream);

                    _logger.LogInformation("Waiting for control to connect to named pipe {0}...", _options.PipeName);
                    await _pipeStream.WaitForConnectionAsync(_listenCts.Token);
                    _logger.LogWarning("Control connected.");

                    await _counterReceiver.Listen(GetStatus, SetStatus, _processCts.Token);
                }
                catch (Exception exception)
                {
                    _processCts?.SafelyCancel();
                    _processCts?.Dispose();
                    _logger.LogWarning(exception, "Waiting for control to connect stopped.");
                }
            }
        }

        private bool GetStatus()
        {
            return _sensorDataReceiver.Status;
        }

        private void SetStatus(bool status)
        {
            if (status)
                _sensorDataReceiver.Start(_interface, _port);
            else
                _sensorDataReceiver.Stop();
        }
    }
}
