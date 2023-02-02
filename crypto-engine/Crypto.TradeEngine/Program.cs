using System.Reflection;
using Crypto.TradeEngine.Scene;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

namespace Crypto.TradeEngine;

public static class Program
{
    public static async Task Main(params string[] args)
    {
        string environment = SetupEnvironment();
        SerilogConfig.Setup(Assembly.GetExecutingAssembly(), environment);
        IConfiguration configuration = GetConfig(args);
        IServiceProvider serviceProvider = new Startup(configuration).ConfigureServices();

        ILoggerFactory loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        Microsoft.Extensions.Logging.ILogger logger = loggerFactory.CreateLogger(typeof(Program));
        using IPlay play = serviceProvider.GetRequiredService<IPlay>();

        try
        {
            logger.LogInformation("Starting in {Environment} environment...", environment);
            await play.Start();
            logger.LogInformation("Stopped");
        }
        catch (Exception exception)
        {
            logger.LogCritical(exception, "Failed to start");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    private static string SetupEnvironment()
    {
        const string envVarName = "ENVIRONMENT";
        string? environment = Environment.GetEnvironmentVariable(envVarName);
        if (string.IsNullOrWhiteSpace(environment))
        {
            environment = "Production";
            Environment.SetEnvironmentVariable(envVarName, environment);
            Console.WriteLine("Set missing env var {0} to {1}.", envVarName, environment);
        }

        return environment;
    }

    private static IConfiguration GetConfig(string[] args)
    {
        ConfigurationBuilder configBuilder = new();
        configBuilder.AddJsonFile("appsettings.json", optional: false);
        configBuilder.AddJsonFile("symbols.json", optional: false);
        configBuilder.AddUserSecrets(Assembly.GetExecutingAssembly(), optional: false);
        configBuilder.AddEnvironmentVariables();
        configBuilder.AddCommandLine(args);

        IConfiguration config = configBuilder.Build();
        return config;
    }
}
