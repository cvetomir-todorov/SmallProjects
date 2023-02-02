using Crypto.TradeEngine.Scene.Actors;
using Microsoft.Extensions.Configuration;

namespace Crypto.Testing.Infra;

public class Instrumentation
{
    public Instrumentation()
    {
        Configuration = new ConfigurationBuilder()
            .AddJsonFile("testsettings.json", optional: false)
            .Build();

        RabbitMqOptions rabbitMqOptions = new();
        Configuration.GetSection("RabbitMQ").Bind(rabbitMqOptions);

        DrainConsumer = new DrainConsumer(rabbitMqOptions);
        TradeConsumer = new TradeConsumer(rabbitMqOptions);
    }

    public IConfiguration Configuration { get; }
    public DrainConsumer DrainConsumer { get; }
    public TradeConsumer TradeConsumer { get; }
}
