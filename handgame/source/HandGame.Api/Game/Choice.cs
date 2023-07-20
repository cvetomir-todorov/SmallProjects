namespace HandGame.Api.Game;

public sealed class Choice
{
    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public override string ToString()
    {
        return $"[{Id}: {Name}]";
    }
}
