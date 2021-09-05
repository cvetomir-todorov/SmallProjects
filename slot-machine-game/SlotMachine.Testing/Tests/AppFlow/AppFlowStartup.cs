using Microsoft.Extensions.DependencyInjection;
using SlotMachine.App;
using SlotMachine.App.Game;
using SlotMachine.App.Game.Spinning;
using SlotMachine.App.Game.UI;
using SlotMachine.Testing.Infrastructure.Mocks;

namespace SlotMachine.Testing.Tests.AppFlow
{
    public class AppFlowStartup : Startup
    {
        protected override void ConfigureCustom(ServiceCollection services)
        {
            // override the game engine
            services.AddScoped<AppFlowGameEngine>();
            services.AddScoped<IGameEngine>(serviceProvider => serviceProvider.GetRequiredService<AppFlowGameEngine>());

            // override the random number generator
            services.AddScoped<RecordedRandomNumberGenerator>();
            services.AddScoped<IRandomNumberGenerator>(serviceProvider => serviceProvider.GetRequiredService<RecordedRandomNumberGenerator>());

            // override the interaction
            services.AddScoped<RecordedInteraction>();
            services.AddScoped<IInteraction>(serviceProvider => serviceProvider.GetRequiredService<RecordedInteraction>());

            // override the output
            services.AddScoped<RecordedOutput>();
            services.AddScoped<IOutput>(serviceProvider => serviceProvider.GetRequiredService<RecordedOutput>());
        }
    }
}
