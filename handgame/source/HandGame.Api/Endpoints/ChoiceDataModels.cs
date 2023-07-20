using System.Text.Json.Serialization;

namespace HandGame.Api.Endpoints;

public sealed class ChoiceDto
{
    [JsonPropertyName("id")]
    public int Id { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    public static readonly ChoiceDto Empty = new() { Id = -1, Name = string.Empty };
}

public sealed class ChoicesResponse
{
    [JsonPropertyName("choices")]
    public ChoiceDto[] Choices { get; init; } = Array.Empty<ChoiceDto>();
}

public sealed class RandomChoiceResponse
{
    [JsonPropertyName("choice")]
    public ChoiceDto Choice { get; init; } = ChoiceDto.Empty;
}
