using System;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using InSync.SensorDataMessaging;
using InSync.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace InSync.Sensor.Data
{
    /// <summary>
    /// Sends sensor data to the counter for e predefined interval of time.
    /// </summary>
    public interface ISensorDataSender : IDisposable
    {
        void Start(byte deviceID, string host, int port);

        void Stop();
    }

    public sealed class SensorDataSender : ISensorDataSender
    {
        private readonly ILogger _logger;
        private readonly SensorDataSenderOptions _options;
        private readonly byte[] _buffer;
        private readonly Random _dataGenerator;
        private readonly object _dataGeneratorLock;
        private readonly SemaphoreSlim _networkSemaphore;
        private Timer _reconnectTimer;
        private Timer _sendDataTimer;
        private CancellationTokenSource _cts;
        private TcpClient _tcpClient;
        private NetworkStream _networkStream;
        private volatile int _isReconnecting;

        public SensorDataSender(ILogger<SensorDataSender> logger, IOptions<SensorDataSenderOptions> options)
        {
            _logger = logger;
            _options = options.Value;

            _buffer = new byte[SensorDataConstants.SensorDataSize];
            _dataGenerator = new Random();
            _dataGeneratorLock = new object();
            _networkSemaphore = new SemaphoreSlim(initialCount: 1, maxCount: 1);
        }

        public void Dispose()
        {
            Stop();
        }

        public void Start(byte deviceID, string host, int port)
        {
            _cts = new CancellationTokenSource();
            _reconnectTimer = new Timer(
                async state => await EnsureConnected(host, port, _cts.Token),
                state: null, dueTime: TimeSpan.Zero, _options.ReconnectInterval);
            _sendDataTimer = new Timer(
                async state => await SendData(deviceID, _cts.Token),
                state: null, dueTime: TimeSpan.Zero, _options.SendDataInterval);
        }

        public void Stop()
        {
            _cts.SafelyCancel();

            _cts?.Dispose();
            _reconnectTimer?.Dispose();
            _sendDataTimer?.Dispose();
            _tcpClient?.Dispose();
            _networkStream?.Dispose();

            _networkSemaphore.Dispose();
        }

        private async Task EnsureConnected(string host, int port, CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
                return;
            // make sure only 1 thread is reconnecting
            if (Interlocked.CompareExchange(ref _isReconnecting, ThreadingConstants.True, ThreadingConstants.False) == ThreadingConstants.True)
                return;

            try
            {
                await Connect(host, port, ct);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to connect.");
            }
            finally
            {
                Interlocked.Exchange(ref _isReconnecting, ThreadingConstants.False);
            }
        }

        private async Task Connect(string host, int port, CancellationToken ct)
        {
            try
            {
                await _networkSemaphore.WaitAsync(ct);

                if (ct.IsCancellationRequested)
                    return;
                // the Connected property just checks the last known state of the underlying socket
                if (_tcpClient != null && _tcpClient.Connected)
                    return;

                _tcpClient?.Dispose();
                _tcpClient = null;
                _networkStream?.Dispose();
                _networkStream = null;

                _tcpClient = new TcpClient();
                _tcpClient.NoDelay = true;
                _tcpClient.SendTimeout = (int) _options.SendDataTimeout.TotalMilliseconds;

                _logger.LogInformation("Connecting to {0}:{1}...", host, port);

                await _tcpClient.ConnectAsync(host, port);
                _networkStream = _tcpClient.GetStream();

                _logger.LogInformation("Connected.");
            }
            finally
            {
                _networkSemaphore.Release();
            }
        }

        private async Task SendData(byte deviceID, CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
                return;

            try
            {
                PopulateData(deviceID);
                await DoSendData(ct);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to send data.");
            }
        }

        private void PopulateData(byte deviceID)
        {
            byte sensorData;
            lock (_dataGeneratorLock)
            {
                sensorData = (byte)_dataGenerator.Next();
            }

            _buffer[0] = 0xAA;
            _buffer[1] = deviceID;
            _buffer[2] = sensorData;
        }

        private async Task DoSendData(CancellationToken ct)
        {
            try
            {
                await _networkSemaphore.WaitAsync(ct);

                if (ct.IsCancellationRequested)
                    return;
                if (_networkStream == null)
                    return;

                await _networkStream.WriteAsync(_buffer, ct);
                await _networkStream.FlushAsync(ct);
            }
            finally
            {
                _networkSemaphore.Release();
            }
        }
    }
}
