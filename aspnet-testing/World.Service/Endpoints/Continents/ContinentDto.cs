namespace World.Service.Endpoints.Continents;

public sealed class ContinentDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Popularity { get; set; }

    public override string ToString()
    {
        return $"{nameof(Id)}:{Id} {nameof(Name)}:{Name} {nameof(Popularity)}:{Popularity}";
    }
}
