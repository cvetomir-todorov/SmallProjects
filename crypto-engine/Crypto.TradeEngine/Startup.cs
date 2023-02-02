using Crypto.TradeEngine.Scene;
using Crypto.TradeEngine.Scene.Actors;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Crypto.TradeEngine;

public class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public IServiceProvider ConfigureServices()
    {
        IServiceCollection services = new ServiceCollection();

        services.AddLogging(logging =>
        {
            logging.AddSerilog();
        });

        // common
        services.AddSingleton<ChannelContainer>();
        services.Configure<SymbolConfigOptions>(_configuration.GetSection("SymbolConfig"));
        services.AddSingleton<IPlay, Play>();
        services.Configure<ConsoleOptions>(_configuration.GetSection("Console"));
        services.AddSingleton<IDateTime, SystemDateTime>();

        // binance
        services.AddSingleton<ITradeFeed, BinanceTradeFeed>();
        services.AddSingleton<IActor>(x => x.GetRequiredService<ITradeFeed>());
        services.Configure<BinanceOptions>(_configuration.GetSection("Binance"));

        // rabbitmq
        services.AddSingleton<RabbitMqBackend>();
        services.AddSingleton<IActor>(x => x.GetRequiredService<RabbitMqBackend>());
        services.Configure<RabbitMqOptions>(_configuration.GetSection("RabbitMQ"));

        // local file
        services.AddSingleton<LocalFile>();
        services.AddSingleton<IActor>(x => x.GetRequiredService<LocalFile>());
        services.Configure<LocalFileOptions>(_configuration.GetSection("LocalFile"));

        // volume spikes
        services.AddSingleton<VolumeSpikeDetector>();
        services.AddSingleton<IActor>(x => x.GetRequiredService<VolumeSpikeDetector>());

        ConfigureServicesCustom(services);

        return services.BuildServiceProvider();
    }

    protected virtual void ConfigureServicesCustom(IServiceCollection services)
    { }
}
