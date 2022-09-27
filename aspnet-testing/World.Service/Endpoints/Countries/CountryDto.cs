using World.Service.Endpoints.Continents;

namespace World.Service.Endpoints.Countries;

public sealed class CountryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Population { get; set; }
    public ContinentDto? Continent { get; set; }

    public override string ToString()
    {
        return $"{nameof(Id)}:{Id} {nameof(Name)}:{Name} {nameof(Population)}:{Population} {nameof(Continent)}:[{Continent}]";
    }
}
