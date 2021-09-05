using System;

namespace InSync.Api.Sensor
{
    public sealed class SensorOptions
    {
        public TimeSpan PersistInterval { get; set; }
        public string DataDirectory { get; set; }
    }
}
