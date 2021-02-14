using CommandLine;

namespace InSync.Counter
{
    public sealed class CounterCommandLine
    {
        [Option('i', "interface", Required = true)]
        public string Interface { get; set; }

        [Option('p', "port", Required = true)]
        public int Port { get; set; }
    }
}
