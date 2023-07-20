using System.Diagnostics.Metrics;

namespace HandGame.Api.Telemetry;

public sealed class GameTelemetry : IDisposable
{
    private readonly Meter _meter;
    private readonly Counter<long> _wins;
    private readonly Counter<long> _ties;
    private readonly Counter<long> _losses;

    public GameTelemetry()
    {
        _meter = new Meter(GameInstrumentation.ActivitySource.Name);

        _wins = _meter.CreateCounter<long>(GameInstrumentation.Metrics.Wins, description: GameInstrumentation.Metrics.WinsDescription);
        _ties = _meter.CreateCounter<long>(GameInstrumentation.Metrics.Ties, description: GameInstrumentation.Metrics.TiesDescription);
        _losses = _meter.CreateCounter<long>(GameInstrumentation.Metrics.Losses, description: GameInstrumentation.Metrics.LossesDescription);
    }

    public void Dispose()
    {
        // implement the full dispose pattern if the class is unsealed
        _meter.Dispose();
    }

    public void NotifyWin()
    {
        _wins.Add(1);
    }

    public void NotifyTie()
    {
        _ties.Add(1);
    }

    public void NotifyLoss()
    {
        _losses.Add(1);
    }
}
