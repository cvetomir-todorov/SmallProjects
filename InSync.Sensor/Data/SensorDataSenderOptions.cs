using System;

namespace InSync.Sensor.Data
{
    public sealed class SensorDataSenderOptions
    {
        public TimeSpan SendDataInterval { get; set; }

        public TimeSpan ReconnectInterval { get; set; }

        public TimeSpan SendDataTimeout { get; set; }
    }
}
