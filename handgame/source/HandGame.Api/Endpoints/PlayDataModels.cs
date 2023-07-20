using System.Text.Json.Serialization;
using FluentValidation;
using HandGame.Api.Game;

namespace HandGame.Api.Endpoints;

public sealed class PlayRequest
{
    [JsonPropertyName("player")]
    public int Player { get; init; }
}

public sealed class PlayRequestValidator : AbstractValidator<PlayRequest>
{
    public PlayRequestValidator(IGameInfo gameInfo)
    {
        RuleFor(x => x.Player).InclusiveBetween(from: 0, to: gameInfo.ChoiceCount - 1);
    }
}

public sealed class PlayResponse
{
    [JsonPropertyName("player")]
    public int Player { get; init; }

    [JsonPropertyName("bot")]
    public int Bot { get; init; }

    [JsonPropertyName("result")]
    public string Result { get; init; } = string.Empty;
}
