namespace Crypto.TradeEngine.Scene.Actors;

public sealed class RabbitMqOptions
{
    public string Hostname { get; init; } = "localhost";
    public int Port { get; init; } = 5672;
    public string VirtualHost { get; init; } = string.Empty;
    public TimeSpan ConnectTimeout { get; init; } = TimeSpan.FromSeconds(5);
    public string UserName { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
}
