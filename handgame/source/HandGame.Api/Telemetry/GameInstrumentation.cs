using System.Diagnostics;
using System.Reflection;

namespace HandGame.Api.Telemetry;

public static class GameInstrumentation
{
    private static readonly string ActivitySourceName = "OpenTelemetry.Instrumentation.HandGame";
    private static readonly AssemblyName AssemblyName = typeof(GameInstrumentation).Assembly.GetName();
    private static readonly Version ActivitySourceVersion = AssemblyName.Version!;

    internal static readonly ActivitySource ActivitySource = new(ActivitySourceName, ActivitySourceVersion.ToString());

    public static class Metrics
    {
        public const string Wins = "handgame.wins";
        public const string WinsDescription = "measures how many wins there are for the players of the game";

        public const string Ties = "handgame.ties";
        public const string TiesDescription = "measures how many ties there are for the players of the game";

        public const string Losses = "handgame.losses";
        public const string LossesDescription = "measures how many losses there are for the players of the game";
    }
}
