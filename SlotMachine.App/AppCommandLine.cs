using CommandLine;

namespace SlotMachine.App
{
    public sealed class AppCommandLine
    {
        [Option('m', "machine-id", Required = true)]
        public string MachineID { get; set; }

        [Option('e', "environment", Required = true)]
        public string Environment { get; set; }

        [Option('c', "game-config", Default = "game.json")]
        public string GameConfig { get; set; } = "game.json";

        [Option('l', "logging-config", Default = "logging.json")]
        public string LoggingConfig { get; set; } = "logging.json";

        public override string ToString()
            => $"[{MachineID}@{Environment}, {nameof(GameConfig)}:{GameConfig}, {nameof(LoggingConfig)}:{LoggingConfig}]";
    }
}
