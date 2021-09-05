using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace InSync.SensorDataStorage
{
    /// <summary>
    /// Supports persisting sensor data.
    /// </summary>
    public interface ISensorDataWriter
    {
        Task WriteData(
            IDictionary<byte, ICollection<byte>> data, IDictionary<byte, byte> averages, string filePath, CancellationToken ct);
    }

    public sealed class SensorDataWriter : ISensorDataWriter
    {
        public Task WriteData(
            IDictionary<byte, ICollection<byte>> data, IDictionary<byte, byte> averages, string filePath, CancellationToken ct)
        {
            string directory = Path.GetDirectoryName(filePath);
            if (string.IsNullOrWhiteSpace(directory))
            {
                throw new ArgumentException("File path should be fully qualified.", filePath);
            }

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            return DoWriteData(data, averages, filePath);
        }

        private static async Task DoWriteData(
            IDictionary<byte, ICollection<byte>> data, IDictionary<byte, byte> averages, string filePath)
        {
            await using FileStream fileStream = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write, FileShare.None);
            await using BinaryWriter writer = new BinaryWriter(fileStream, Encoding.UTF8);

            int sensorCount = data.Count;
            writer.Write(sensorCount);

            foreach (KeyValuePair<byte, ICollection<byte>> pair in data)
            {
                byte deviceID = pair.Key;
                ICollection<byte> sensorValues = pair.Value;
                byte average = averages[deviceID];

                writer.Write(deviceID);
                writer.Write(average);
                writer.Write(sensorValues.Count);

                foreach (byte sensorValue in sensorValues)
                {
                    writer.Write(sensorValue);
                }
            }

            writer.Flush();
        }
    }
}
