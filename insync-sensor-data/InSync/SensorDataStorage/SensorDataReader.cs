using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace InSync.SensorDataStorage
{
    /// <summary>
    /// Support read/find operations on the persisted sensor data.
    /// </summary>
    public interface ISensorDataReader
    {
        Task<SensorDataResult> FindSensorData(byte deviceID, string filePath);
    }

    public sealed class SensorDataResult
    {
        public SensorDataResult()
        {
            IsFound = false;
        }

        public SensorDataResult(byte average, byte[] sensorValues)
        {
            IsFound = true;
            Average = average;
            SensorValues = sensorValues;
        }

        public bool IsFound { get; private set; }
        public byte Average { get; private set; }
        public byte[] SensorValues { get; private set; }
    }

    public sealed class SensorDataReader : ISensorDataReader
    {
        public async Task<SensorDataResult> FindSensorData(byte deviceID, string filePath)
        {
            await using FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using BinaryReader reader = new BinaryReader(fileStream, Encoding.UTF8);

            int sensorCount = reader.ReadInt32();
            bool isFound = false;
            byte average = 0;
            byte[] sensorValues = null;

            for (int i = 0; i < sensorCount; ++i)
            {
                byte currentDeviceID = reader.ReadByte();
                if (currentDeviceID != deviceID)
                {
                    // skip the average byte
                    reader.BaseStream.Position += 1;
                    int valuesCount = reader.ReadInt32();
                    // skip the sensor values
                    reader.BaseStream.Position += valuesCount;
                }
                else
                {
                    isFound = true;
                    average = reader.ReadByte();
                    int valuesCount = reader.ReadInt32();
                    sensorValues = new byte[valuesCount];
                    reader.Read(sensorValues, 0, valuesCount);
                    break;
                }
            }

            SensorDataResult result = isFound ? new SensorDataResult(average, sensorValues) : new SensorDataResult();
            return result;
        }
    }
}
