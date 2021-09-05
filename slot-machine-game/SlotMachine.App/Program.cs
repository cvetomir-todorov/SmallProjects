using System;
using CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SlotMachine.App.Game;

namespace SlotMachine.App
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            Parser.Default
                .ParseArguments<AppCommandLine>(args)
                .WithParsed(Start);
        }

        private static void Start(AppCommandLine commandLine)
        {
            IServiceProvider serviceProvider = new Startup().ConfigureServices(commandLine);
            IServiceScope scope = serviceProvider.CreateScope();
            ILoggerFactory loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
            ILogger logger = loggerFactory.CreateLogger(typeof(Program));

            try
            {
                IGameEngine gameEngine = scope.ServiceProvider.GetRequiredService<IGameEngine>();
                gameEngine.Start();
            }
            catch (Exception exception)
            {
                // this is our global exception handler
                logger.LogError(exception, "Application failed.");
            }
            finally
            {
                scope.Dispose();
            }
        }
    }
}
