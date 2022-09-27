using Microsoft.EntityFrameworkCore;
using World.Service.Database.Continents;

namespace World.Service.Database.Countries;

public class CountryRepo : ICountryRepo
{
    private readonly WorldDbContext _dbContext;

    public CountryRepo(WorldDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<CountryEntity>> GetAllCountries()
    {
        return await _dbContext.Countries
            .Include(c => c.Continent)
            .ToListAsync();
    }

    public async Task<CountryEntity?> GetSingleCountry(int id)
    {
        return await _dbContext.Countries
            .Include(c => c.Continent)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<CountryEntity> AddCountry(string name, int population, ContinentEntity continent)
    {
        CountryEntity country = new()
        {
            Name = name,
            Population = population,
            Continent = continent
        };

        _dbContext.Countries.Add(country);
        await _dbContext.SaveChangesAsync();

        return country;
    }
}
