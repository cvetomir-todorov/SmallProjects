namespace World.Service.Database.Continents;

public sealed class ContinentEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Popularity { get; set; }
}
