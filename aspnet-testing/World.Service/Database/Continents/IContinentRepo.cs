namespace World.Service.Database.Continents;

public interface IContinentRepo
{
    Task<IEnumerable<ContinentEntity>> GetAllContinents();

    Task<IEnumerable<ContinentEntity>> GetTopContinents(int topCount);

    Task<ContinentEntity> AddContinent(string name, int popularity);
}