using System;
using CommandLine;
using InSync.Counter.Control;
using InSync.Counter.SensorData;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace InSync.Counter
{
    public static class CounterProgram
    {
        public static void Main(string[] args)
        {
            Parser.Default
                .ParseArguments<CounterCommandLine>(args)
                .WithParsed(Start);
        }

        private static void Start(CounterCommandLine commandLine)
        {
            CounterStartup startup = new CounterStartup();
            IServiceProvider serviceProvider = startup.ConfigureServices();
            IServiceScope scope = null;
            ILogger logger = null;
            int exitCode = 0;

            try
            {
                scope = serviceProvider.CreateScope();
                ILoggerFactory loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
                logger = loggerFactory.CreateLogger(typeof(CounterProgram));

                Start(commandLine, logger, scope);
            }
            // global exception handling so the app doesn't crash
            catch (Exception exception)
            {
                if (logger != null)
                {
                    logger.LogError(exception, "Unexpected error.");
                }
                else
                {
                    Console.WriteLine("Unexpected error: {0}", exception);
                }

                exitCode = 1;
            }
            finally
            {
                scope?.Dispose();
                LogManager.Shutdown();
            }

            Environment.Exit(exitCode);
        }

        private static void Start(CounterCommandLine commandLine, ILogger logger, IServiceScope scope)
        {
            ISensorDataProcessor sensorDataProcessor = scope.ServiceProvider.GetRequiredService<ISensorDataProcessor>();
            sensorDataProcessor.Start();
            ISensorDataReceiver sensorDataReceiver = scope.ServiceProvider.GetRequiredService<ISensorDataReceiver>();
            sensorDataReceiver.Start(commandLine.Interface, commandLine.Port);
            IControlReceiver controlReceiver = scope.ServiceProvider.GetRequiredService<IControlReceiver>();
            controlReceiver.Start(commandLine.Interface, commandLine.Port);

            logger.LogInformation("Choose an action - [e]xit | [s]tart | s[t]op (casing matters!)");
            while (true)
            {
                string input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input) || input.StartsWith('e'))
                {
                    break;
                }

                if (input.StartsWith('s'))
                {
                    sensorDataReceiver.Start(commandLine.Interface, commandLine.Port);
                }
                if (input.StartsWith('t'))
                {
                    sensorDataReceiver.Stop();
                }
            }
        }
    }
}
