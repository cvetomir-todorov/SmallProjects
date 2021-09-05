using System;
using System.Buffers;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using InSync.SensorDataMessaging;
using InSync.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace InSync.Counter.SensorData
{
    /// <summary>
    /// Receives data coming from multiple sensors.
    /// </summary>
    public interface ISensorDataReceiver : IDisposable
    {
        void Start(string @interface, int port);

        void Stop();

        int ConnectedSensors { get; }

        bool Status { get; }
    }

    public sealed class SensorDataReceiver : ISensorDataReceiver
    {
        private readonly ILogger _logger;
        private readonly ISensorData _sensorData;
        private readonly SensorDataOptions _options;
        private readonly ArrayPool<byte> _bufferPool;
        private readonly object _stateLock;
        private bool _isStarted;
        private TcpListener _listener;
        private CancellationTokenSource _cts;
        private int _connectedSensors;

        public SensorDataReceiver(ILogger<SensorDataReceiver> logger, ISensorData sensorData, IOptions<SensorDataOptions> options)
        {
            _logger = logger;
            _sensorData = sensorData;
            _options = options.Value;
            _bufferPool = ArrayPool<byte>.Create(
                maxArrayLength: SensorDataConstants.SensorDataSize,
                maxArraysPerBucket: _options.BufferPoolSize);
            _stateLock = new object();
        }

        public void Dispose()
        {
            Stop();
        }

        public void Start(string @interface, int port)
        {
            lock (_stateLock)
            {
                if (_isStarted)
                {
                    _logger.LogInformation("Already started.");
                    return;
                }

                _listener = new TcpListener(IPAddress.Parse(@interface), port);
                _cts = new CancellationTokenSource();

                try
                {
                    _listener.Start();
                    _logger.LogInformation("Listening for clients on {0}:{1}.", @interface, port);
                    Task _ = Listen(_cts.Token);
                }
                catch (SocketException socketException)
                {
                    _logger.LogError(socketException, "Failed to start listening for clients on {0}:{1}.", @interface, port);
                }
                finally
                {
                    _isStarted = true;
                    _logger.LogInformation("Started.");
                }
            }
        }

        public void Stop()
        {
            lock (_stateLock)
            {
                if (!_isStarted)
                {
                    _logger.LogInformation("Already stopped.");
                    return;
                }

                try
                {
                    _cts?.SafelyCancel();
                    _cts?.Dispose();
                    _listener?.Stop();
                }
                catch (ObjectDisposedException)
                {
                    // already disposed, simply ignore
                }
                finally
                {
                    _isStarted = false;
                    _logger.LogWarning("Stopped.");
                }
            }
        }

        private async Task Listen(CancellationToken ct)
        {
            try
            {
                while (!ct.IsCancellationRequested)
                {
                    TcpClient sensorClient = await _listener.AcceptTcpClientAsync();
                    _logger.LogInformation("Client {0} connected.", sensorClient.Client.RemoteEndPoint);
                    Task _ = Task.Run(() => ProcessClient(sensorClient, ct), ct);
                }
            }
            catch (ObjectDisposedException)
            {
                // ignore that the listener has been disposed since that's what's expected to happen when the instance is called Stop()
            }
            catch (Exception exception)
            {
                _logger.LogCritical(exception, "Listening for clients failed.");
            }
        }

        public int ConnectedSensors => _connectedSensors;

        public bool Status => _isStarted;

        private async Task ProcessClient(TcpClient sensorClient, CancellationToken ct)
        {
            byte[] buffer = null;
            EndPoint sensorAddress = sensorClient.Client.RemoteEndPoint;

            try
            {
                buffer = _bufferPool.Rent(SensorDataConstants.SensorDataSize);
                Interlocked.Increment(ref _connectedSensors);
                sensorClient.ReceiveTimeout = (int)_options.SensorIdleTimeout.TotalMilliseconds;

                await DoProcessClient(sensorClient, buffer, ct);
            }
            // global exception handler for client
            catch (Exception exception)
            {
                _logger.LogError(exception, "Client {0} error.", sensorClient.Client.RemoteEndPoint);
            }
            finally
            {
                Interlocked.Decrement(ref _connectedSensors);
                if (buffer != null)
                {
                    _bufferPool.Return(buffer);
                }
                sensorClient.Dispose();
                _logger.LogInformation("Client {0} disconnected.", sensorAddress);
            }
        }

        private async Task DoProcessClient(TcpClient sensorClient, byte[] buffer, CancellationToken ct)
        {
            if (ct.IsCancellationRequested)
                return;

            await using NetworkStream clientStream = sensorClient.GetStream();

            while (!ct.IsCancellationRequested)
            {
                int bytesRead = await ReadBytes(clientStream, buffer, 0, SensorDataConstants.SensorDataSize, ct);
                if (bytesRead < 0)
                {
                    // network connection is closed
                    break;
                }
                ProcessMessage(buffer);
            }
        }

        private static async Task<int> ReadBytes(Stream source, byte[] buffer, int offset, int count, CancellationToken ct)
        {
            int totalBytesRead = 0;
            int bytesRemaining = count;

            while (totalBytesRead < count)
            {
                int bytesRead = await source.ReadAsync(buffer, offset, bytesRemaining, ct);
                if (bytesRead == 0)
                {
                    // network connection is closed
                    return -1;
                }

                totalBytesRead += bytesRead;
                offset += bytesRead;
                bytesRemaining -= bytesRead;
            }

            return totalBytesRead;
        }

        private void ProcessMessage(byte[] buffer)
        {
            byte deviceID = buffer[1];
            byte sensorData = buffer[2];

            _sensorData.AddData(deviceID, sensorData);
            _logger.LogTrace("Device {0:###}: {1}", deviceID, sensorData);
        }
    }
}
