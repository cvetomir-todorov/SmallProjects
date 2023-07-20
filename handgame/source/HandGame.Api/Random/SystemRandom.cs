namespace HandGame.Api.Random;

public class SystemRandom : IRandom
{
    private readonly System.Random _random;

    public SystemRandom()
    {
        _random = new System.Random();
    }

    public Task<int> GenerateInt32(int min, int max)
    {
        if (min >= max)
        {
            throw new ArgumentException($"Min {min} should be strictly < max {max}.");
        }

        // maxValue is exclusive
        int value = _random.Next(minValue: min, maxValue: max);
        return Task.FromResult(value);
    }
}
