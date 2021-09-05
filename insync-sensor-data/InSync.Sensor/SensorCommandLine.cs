using CommandLine;

namespace InSync.Sensor
{
    public sealed class SensorCommandLine
    {
        [Option('d', "deviceID", Required = true)]
        public byte DeviceID { get; set; }

        [Option('h', "host", Required = true)]
        public string Host { get; set; }

        [Option('p', "port", Required = true)]
        public int Port { get; set; }
    }
}
