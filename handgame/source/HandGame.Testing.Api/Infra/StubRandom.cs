using HandGame.Api.Random;

namespace HandGame.Testing.Api.Infra;

public class StubRandom : IRandom
{
    public int Value { get; set; }

    public Task<int> GenerateInt32(int min, int max)
    {
        return Task.FromResult(Value);
    }
}
