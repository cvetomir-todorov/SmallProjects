using World.Service.Database.Continents;

namespace World.Service.Database.Countries;

public interface ICountryRepo
{
    public Task<IEnumerable<CountryEntity>> GetAllCountries();

    public Task<CountryEntity?> GetSingleCountry(int id);

    public Task<CountryEntity> AddCountry(string name, int population, ContinentEntity continent);
}
