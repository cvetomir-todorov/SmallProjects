using Crypto.Testing.Infra.Mocks;
using Crypto.TradeEngine;
using Crypto.TradeEngine.Scene;
using Crypto.TradeEngine.Scene.Actors;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Crypto.Testing.Infra;

public class TestStartup : Startup
{
    public TestStartup(IConfiguration configuration) : base(configuration)
    { }

    protected override void ConfigureServicesCustom(IServiceCollection services)
    {
        services.AddSingleton<IPlay, TestPlay>();
        services.AddSingleton<ITradeFeed, TestTradeFeed>();
        services.AddSingleton<IDateTime, TestDateTime>();
    }
}
