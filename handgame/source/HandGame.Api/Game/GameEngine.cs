using HandGame.Api.Random;
using HandGame.Api.Telemetry;

namespace HandGame.Api.Game;

public class GameEngine : IGame
{
    private readonly Choice[] _choices;
    private readonly string[] _results;
    private readonly ILogger _logger;
    private readonly IRandom _random;
    private readonly GameTelemetry _gameTelemetry;

    public GameEngine(ILogger<GameEngine> logger, IRandom random, GameTelemetry gameTelemetry)
    {
        _choices = new[]
        {
            new Choice { Id = 0, Name = "rock" },
            new Choice { Id = 1, Name = "paper" },
            new Choice { Id = 2, Name = "scissors" },
            new Choice { Id = 3, Name = "lizard" },
            new Choice { Id = 4, Name = "spock" }
        };

        // each directed pair (x, y) is uniquely identified via the formula x*n + y,
        // where n is the number of the choices and each choice is strictly in [0, n) range
        // if x != y, then (x, y) != (y, x) because x*n + y != y*n + x
        _results = new[]
        {
            // rock
            /* 0 * n + 0 */ "tie",
            /* 0 * n + 1 */ "lose", // rock is covered by paper
            /* 0 * n + 2 */ "win", // rock crushes scissors
            /* 0 * n + 3 */ "win", // rock crushes lizard
            /* 0 * n + 4 */ "lose", // rock is vaporized by spock
            // paper
            /* 1 * n + 0 */ "win", // paper covers rock
            /* 1 * n + 1 */ "tie",
            /* 1 * n + 2 */ "lose", // paper is cut by scissors
            /* 1 * n + 3 */ "lose", // paper is eaten by lizard
            /* 1 * n + 4 */ "win", // paper disproves spock
            // scissors
            /* 2 * n + 0 */ "lose", // scissors are crushed by rock
            /* 2 * n + 1 */ "win", // scissors cut paper
            /* 2 * n + 2 */ "tie",
            /* 2 * n + 3 */ "win", // scissors decapitate lizard
            /* 2 * n + 4 */ "lose", // scissors are smashed by spock
            // lizard
            /* 3 * n + 0 */ "lose", // lizard is crushed by rock
            /* 3 * n + 1 */ "win", // lizard eats paper
            /* 3 * n + 2 */ "lose", // lizard is decapitated by scissors
            /* 3 * n + 3 */ "tie",
            /* 3 * n + 4 */ "win", // lizard poisons spock
            // spock
            /* 4 * n + 0 */ "win", // spock vaporizes rock
            /* 4 * n + 1 */ "lose", // spock is disproved by paper
            /* 4 * n + 2 */ "win", // spock smashes scissors
            /* 4 * n + 3 */ "lose", // spock is poisoned by lizard
            /* 4 * n + 4 */ "tie"
        };

        _logger = logger;
        _random = random;
        _gameTelemetry = gameTelemetry;
    }

    public IEnumerable<Choice> GetAllChoices()
    {
        foreach (Choice choice in _choices)
        {
            yield return choice;
        }
    }

    public int ChoiceCount => _choices.Length;

    public async Task<Choice> GetRandomChoice()
    {
        int randomValue = await GetChoice();
        return _choices[randomValue];
    }

    public async Task<PlayResult> Play(int playerChoice)
    {
        if (playerChoice < 0 || playerChoice >= _choices.Length)
        {
            throw new ArgumentException($"Player choice should be within [0, {_choices.Length - 1}].");
        }

        int botChoice = await GetChoice();
        int directedPair = playerChoice * _choices.Length + botChoice;
        string result = _results[directedPair];
        UpdateMetrics(result);

        return new PlayResult
        {
            BotChoice = botChoice,
            Result = result
        };
    }

    private void UpdateMetrics(string result)
    {
        switch (result)
        {
            case "win":
                _gameTelemetry.NotifyWin();
                break;
            case "tie":
                _gameTelemetry.NotifyTie();
                break;
            case "lose":
                _gameTelemetry.NotifyLoss();
                break;
            default:
                _logger.LogWarning("Unexpected game result {GameResult}", result);
                break;
        }
    }

    private async Task<int> GetChoice()
    {
        const int min = 0;
        int max = _choices.Length - 1;

        int randomValue = await _random.GenerateInt32(min, max);
        if (randomValue < 0 || randomValue > max)
        {
            throw new InvalidOperationException($"Generated random value for bot choice should be within [{min}, {max}].");
        }

        return randomValue;
    }
}
