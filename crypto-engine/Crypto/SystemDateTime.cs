namespace Crypto;

public interface IDateTime
{
    DateTime UtcNow();
}

public class SystemDateTime : IDateTime
{
    public DateTime UtcNow() => DateTime.UtcNow;
}
