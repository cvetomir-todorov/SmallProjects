namespace World.Service.Endpoints.Countries;

public sealed class CountriesResponse
{
    public CountryDto[] Countries { get; set; } = Array.Empty<CountryDto>();
}
