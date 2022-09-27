namespace World.Service.Endpoints.Continents;

public sealed class ContinentsResponse
{
    public ContinentDto[] Continents { get; set; } = Array.Empty<ContinentDto>();
}
