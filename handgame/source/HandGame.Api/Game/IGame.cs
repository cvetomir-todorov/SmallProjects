namespace HandGame.Api.Game;

public interface IGameInfo
{
    public IEnumerable<Choice> GetAllChoices();

    public int ChoiceCount { get; }
}

public interface IGame : IGameInfo
{
    public Task<Choice> GetRandomChoice();

    public Task<PlayResult> Play(int playerChoice);
}

public readonly struct PlayResult
{
    public int BotChoice { get; init; }
    
    public string Result { get; init; }
}
