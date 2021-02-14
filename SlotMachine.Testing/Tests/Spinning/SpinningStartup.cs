using Microsoft.Extensions.DependencyInjection;
using SlotMachine.App;
using SlotMachine.App.Game.Spinning;
using SlotMachine.App.Game.UI;
using SlotMachine.Testing.Infrastructure.Mocks;

namespace SlotMachine.Testing.Tests.Spinning
{
    public class SpinningStartup : Startup
    {
        protected override void ConfigureCustom(ServiceCollection services)
        {
            // override the random number generator
            services.AddScoped<RecordedRandomNumberGenerator>();
            services.AddScoped<IRandomNumberGenerator>(serviceProvider => serviceProvider.GetRequiredService<RecordedRandomNumberGenerator>());

            // override the output in order to avoid suspense delay
            services.AddScoped<IOutput, RecordedOutput>();
        }
    }
}
