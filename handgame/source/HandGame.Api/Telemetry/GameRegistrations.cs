using OpenTelemetry.Metrics;

namespace HandGame.Api.Telemetry;

public static class GameRegistrations
{
    public static MeterProviderBuilder AddGameInstrumentation(this MeterProviderBuilder builder)
    {
        return builder.AddMeter(GameInstrumentation.ActivitySource.Name);
    }
}
