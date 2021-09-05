using System;
using CommandLine;
using InSync.Sensor.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace InSync.Sensor
{
    public static class SensorProgram
    {
        public static void Main(string[] args)
        {
            Parser.Default
                .ParseArguments<SensorCommandLine>(args)
                .WithParsed(Start);
        }

        private static void Start(SensorCommandLine commandLine)
        {
            SensorStartup startup = new SensorStartup();
            IServiceProvider serviceProvider = startup.ConfigureServices();
            IServiceScope scope = null;
            ILogger logger = null;
            int exitCode = 0;

            try
            {
                scope = serviceProvider.CreateScope();
                ILoggerFactory loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
                logger = loggerFactory.CreateLogger(typeof(SensorProgram));

                ISensorDataSender sensorDataSender = scope.ServiceProvider.GetRequiredService<ISensorDataSender>();
                sensorDataSender.Start(commandLine.DeviceID, commandLine.Host, commandLine.Port);

                logger.LogInformation("Press ENTER to exit.");
                Console.ReadLine();
                sensorDataSender.Stop();
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
    }
}
