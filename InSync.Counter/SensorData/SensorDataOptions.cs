using System;

namespace InSync.Counter.SensorData
{
    public sealed class SensorDataOptions
    {
        public TimeSpan PersistInterval { get; set; }

        public TimeSpan MinSendInterval { get; set; }

        public TimeSpan SensorIdleTimeout { get; set; }

        public int BufferPoolSize { get; set; }

        public string PersistDirectory { get; set; }
    }
}
