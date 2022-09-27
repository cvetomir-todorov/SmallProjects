using World.Service.Database.Continents;

namespace World.Service.Database.Countries;

public sealed class CountryEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Population { get; set; }
    public ContinentEntity? Continent { get; set; }
}