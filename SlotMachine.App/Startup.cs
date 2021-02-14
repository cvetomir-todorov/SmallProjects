using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using SlotMachine.App.Game;
using SlotMachine.App.Game.Configuration;
using SlotMachine.App.Game.Money;
using SlotMachine.App.Game.Spinning;
using SlotMachine.App.Game.States;
using SlotMachine.App.Game.UI;

namespace SlotMachine.App
{
    public class Startup
    {
        private readonly ServiceCollection _services;

        public Startup()
        {
            _services = new ServiceCollection();
        }

        public IServiceProvider ConfigureServices(AppCommandLine commandLine)
        {
            ConfigureLogging(commandLine, _services);
            ConfigureGameConfiguration(commandLine, _services);
            ConfigureServices(_services);
            ConfigureCustom(_services);

            return _services.BuildServiceProvider();
        }

        private static void ConfigureLogging(AppCommandLine commandLine, ServiceCollection services)
        {
            services.AddLogging(logging =>
            {
                logging.AddSerilog(dispose: true);
            });

            IConfiguration loggingConfiguration = new ConfigurationBuilder()
                .AddJsonFile(commandLine.LoggingConfig, optional: false)
                .AddJsonFile(InsertEnvironment(commandLine.LoggingConfig, commandLine.Environment), optional: true)
                .Build();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(loggingConfiguration)
                .Enrich.WithProperty("MachineID", commandLine.MachineID)
                .CreateLogger();
        }

        private static void ConfigureGameConfiguration(AppCommandLine commandLine, ServiceCollection services)
        {
            IConfiguration gameConfiguration = new ConfigurationBuilder()
                .AddJsonFile(commandLine.GameConfig, optional: false)
                .AddJsonFile(InsertEnvironment(commandLine.GameConfig, commandLine.Environment), optional: true)
                .Build();

            services.Configure<AllOptions>(gameConfiguration);
        }

        private static void ConfigureServices(ServiceCollection services)
        {
            services.AddScoped<IGameEngine, GameEngine>();
            services.AddScoped<Wallet>();
            services.AddScoped<IWallet>(sp => sp.GetRequiredService<Wallet>());
            services.AddScoped<IWalletConfiguration>(sp => sp.GetRequiredService<Wallet>());

            // UI
            services.AddScoped<IOutput, ConsoleOutput>();
            services.AddScoped<IInteraction, ConsoleInteraction>();

            // States
            services.AddScoped<State, DepositOrExitState>();
            services.AddScoped<State, DepositState>();
            services.AddScoped<State, ExitState>();
            services.AddScoped<State, StakeOrWithdrawState>();
            services.AddScoped<State, StakeState>();
            services.AddScoped<State, WithdrawState>();
            services.AddScoped<State, SpinState>();

            // Spinning
            services.AddScoped<Spin>();
            services.AddScoped<ISpin, Spin>(sp => sp.GetRequiredService<Spin>());
            services.AddScoped<ISpinConfiguration, Spin>(sp => sp.GetRequiredService<Spin>());
            services.AddScoped<SymbolGenerator>();
            services.AddScoped<IRandomNumberGenerator, SystemRandomNumberGenerator>();
        }

        private static string InsertEnvironment(string file, string environment)
        {
            string name = Path.GetFileNameWithoutExtension(file);
            string extension = Path.GetExtension(file);
            string result = $"{name}.{environment}{extension}";

            return result;
        }

        protected virtual void ConfigureCustom(ServiceCollection services)
        {}
    }
}
