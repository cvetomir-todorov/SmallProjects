using Microsoft.EntityFrameworkCore;

namespace World.Service.Database.Continents;

public class ContinentRepo : IContinentRepo
{
    private readonly WorldDbContext _dbContext;

    public ContinentRepo(WorldDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IEnumerable<ContinentEntity>> GetAllContinents()
    {
        return await _dbContext.Continents
            .ToListAsync();
    }

    public async Task<IEnumerable<ContinentEntity>> GetTopContinents(int topCount)
    {
        return await _dbContext.Continents
            .OrderByDescending(c => c.Popularity)
            .Take(topCount)
            .ToListAsync();
    }

    public async Task<ContinentEntity> AddContinent(string name, int popularity)
    {
        ContinentEntity continent = new()
        {
            Name = name,
            Popularity = popularity
        };

        _dbContext.Continents.Add(continent);
        await _dbContext.SaveChangesAsync();

        return continent;
    }
}
