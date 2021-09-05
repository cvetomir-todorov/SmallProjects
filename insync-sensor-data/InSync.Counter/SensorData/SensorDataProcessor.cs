using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using InSync.SensorDataStorage;
using InSync.Threading;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace InSync.Counter.SensorData
{
    /// <summary>
    /// Processes data coming from multiple sensors for e predefined interval:
    /// - calculating averages
    /// - persistence
    /// - logging
    /// </summary>
    public interface ISensorDataProcessor : IDisposable
    {
        void Start();
    }

    public sealed class SensorDataProcessor : ISensorDataProcessor
    {
        private readonly ILogger _logger;
        private readonly ISensorDataReceiver _sensorDataReceiver;
        private readonly ISensorData _sensorData;
        private readonly ISensorDataWriter _writer;
        private readonly ISensorDataUtil _sensorDataUtil;
        private readonly SensorDataOptions _options;
        private DateTime _startTime;
        private Stopwatch _stopwatch;
        private Timer _persistTimer;
        private CancellationTokenSource _cts;

        public SensorDataProcessor(
            ILogger<SensorDataProcessor> logger,
            ISensorDataReceiver sensorDataReceiver,
            ISensorData sensorData,
            ISensorDataWriter writer,
            ISensorDataUtil sensorDataUtil,
            IOptions<SensorDataOptions> options)
        {
            _logger = logger;
            _sensorDataReceiver = sensorDataReceiver;
            _sensorData = sensorData;
            _writer = writer;
            _sensorDataUtil = sensorDataUtil;
            _options = options.Value;
        }

        public void Dispose()
        {
            _cts.SafelyCancel();

            _cts?.Dispose();
            _persistTimer?.Dispose();

            _stopwatch.Stop();
        }

        public void Start()
        {
            _startTime = DateTime.UtcNow;
            _stopwatch = Stopwatch.StartNew();

            _cts = new CancellationTokenSource();
            _persistTimer = new Timer(
                async state => await ProcessData(_cts.Token),
                state: null, dueTime: _options.PersistInterval, _options.PersistInterval);
        }

        private async Task ProcessData(CancellationToken ct)
        {
            try
            {
                IDictionary<byte, ICollection<byte>> data = _sensorData.GetData();
                DateTime currentTime = _startTime + _stopwatch.Elapsed;

                IDictionary<byte, byte> averages = CalculateAverages(data);

                if (data.Count > 0) // avoid creating empty files
                {
                    _logger.LogInformation("Persisting sensor data...");
                    await DoPersistData(data, averages, currentTime, ct);
                    _logger.LogInformation("Persisted sensor data.");
                }

                // log that no data is available
                LogData(data, averages);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Failed to persist sensor data.");
            }
        }

        private IDictionary<byte, byte> CalculateAverages(IDictionary<byte, ICollection<byte>> data)
        {
            IDictionary<byte, byte> averages = new Dictionary<byte, byte>(data.Count);

            foreach (KeyValuePair<byte, ICollection<byte>> pair in data)
            {
                byte deviceID = pair.Key;
                ICollection<byte> sensorValues = pair.Value;

                // 120 seconds interval with 1 byte each second = at most 120 bytes, therefore no overflow is expected
                int sum = 0;
                foreach (byte sensorValue in sensorValues)
                    sum += sensorValue;

                // average of bytes should be byte as well
                byte average = (byte) Math.Round((double) sum / sensorValues.Count);

                averages.Add(deviceID, average);
            }

            return averages;
        }

        private Task DoPersistData(
            IDictionary<byte, ICollection<byte>> data, IDictionary<byte, byte> averages, DateTime currentTime, CancellationToken ct)
        {
            string fileName = _sensorDataUtil.GetDataFileName(currentTime, _options.PersistInterval);
            string filePath = Path.Combine(_options.PersistDirectory, fileName);

            Task persistTask = _writer.WriteData(data, averages, filePath, ct);
            return persistTask;
        }

        private void LogData(IDictionary<byte, ICollection<byte>> data, IDictionary<byte, byte> averages)
        {
            _logger.LogInformation("Sensors connected: {0}", _sensorDataReceiver.ConnectedSensors);
            _logger.LogInformation("Sensors reported data: {0}", data.Count);
            StringBuilder logOutput = new StringBuilder();

            foreach (KeyValuePair<byte, ICollection<byte>> pair in data)
            {
                byte deviceID = pair.Key;
                ICollection<byte> sensorValues = pair.Value;
                byte average = averages[deviceID];

                logOutput.AppendFormat("Sensor {0} reported {1} time(s) with average {2}: ", deviceID, sensorValues.Count, average);
                foreach (byte sensorValue in sensorValues)
                {
                    logOutput.Append(sensorValue).Append(' ');
                }

                _logger.LogInformation(logOutput.ToString());
                logOutput.Clear();
            }
        }
    }
}
