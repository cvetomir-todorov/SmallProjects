namespace HandGame.Api.Random;

public sealed class RandomOptions
{
    public string Address { get; init; } = string.Empty;

    // the timeout for a single request attempt
    public TimeSpan AttemptTimeout { get; init; }

    // the sleep intervals after an unsuccessful attempt
    public TimeSpan[] SleepIntervals { get; init; } = Array.Empty<TimeSpan>();

    public string HealthAddress { get; init; } = string.Empty;

    public TimeSpan HealthTimeout { get; init; }
}
