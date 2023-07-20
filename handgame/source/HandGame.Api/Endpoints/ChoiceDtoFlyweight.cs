using System.Collections.Immutable;
using HandGame.Api.Game;

namespace HandGame.Api.Endpoints;

/// <summary>
/// Reduces allocation and subsequent garbage collection of <see cref="ChoiceDto"/> objects which are low of count and immutable.
/// </summary>
public class ChoiceDtoFlyweight
{
    private readonly ImmutableDictionary<int, ChoiceDto> _choiceDtoMap;

    public ChoiceDtoFlyweight(IGameInfo gameInfo)
    {
        _choiceDtoMap = gameInfo.GetAllChoices().ToImmutableDictionary(
            keySelector: choice => choice.Id,
            elementSelector: choice => new ChoiceDto { Id = choice.Id, Name = choice.Name });
    }

    public ChoiceDto GetChoiceDto(int id)
    {
        if (!_choiceDtoMap.TryGetValue(id, out ChoiceDto? choiceDto))
        {
            throw new InvalidOperationException($"Flyweight doesn't contain a choice DTO for id {id}.");
        }

        return choiceDto;
    }
}
