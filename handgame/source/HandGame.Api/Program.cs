using System.Reflection;
using Serilog;

namespace HandGame.Api;

public static class Program
{
    public static void Main(params string[] args)
    {
        Assembly? entryAssembly = Assembly.GetEntryAssembly();
        if (entryAssembly == null)
        {
            throw new InvalidOperationException("Entry assembly is null.");
        }

        const string aspnetEnvVarName = "ASPNETCORE_ENVIRONMENT";
        string? environment = Environment.GetEnvironmentVariable(aspnetEnvVarName);
        if (string.IsNullOrWhiteSpace(environment))
        {
            throw new InvalidOperationException($"Environment variable '{aspnetEnvVarName}' is not set or is whitespace.");
        }

        SerilogConfig.Setup(entryAssembly, environment);
        Serilog.ILogger logger = Log.ForContext(typeof(Program));

        try
        {
            logger.Information("Starting in {Environment} environment", environment);
            CreateHostBuilder(args).Build().Run();
        }
        catch (Exception exception)
        {
            logger.Fatal(exception, "Unexpected failure");
        }
        finally
        {
            logger.Information("Ended");
            Log.CloseAndFlush();
        }
    }

    private static IHostBuilder CreateHostBuilder(params string[] args)
    {
        return Host
            .CreateDefaultBuilder(args)
            .UseSerilog()
            .ConfigureAppConfiguration(configuration =>
            {
                configuration.AddEnvironmentVariables("HANDGAME_");
            })
            .ConfigureWebHostDefaults(webHost =>
            {
                webHost.UseStartup<Startup>();
            });
    }
}
