using NUnit.Framework;
using Serilog;
using Serilog.Events;

namespace HandGame.Testing.Api;

[SetUpFixture]
public class Program
{
    [OneTimeSetUp]
    public void Begin()
    {
        InitEnv();
        ConfigureLogging();
    }

    private static void InitEnv()
    {
        const string envName = "ASPNETCORE_ENVIRONMENT";
        string? env = Environment.GetEnvironmentVariable(envName);
        if (string.IsNullOrWhiteSpace(env))
        {
            env = "Development";
            Environment.SetEnvironmentVariable(envName, env);
        }

        TestContext.Progress.WriteLine($"{envName}: {env}");
    }

    private static void ConfigureLogging()
    {
        LoggerConfiguration loggerConfig = new LoggerConfiguration()
            .MinimumLevel.Is(LogEventLevel.Verbose)
            .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .Destructure.ToMaximumDepth(8)
            .Destructure.ToMaximumStringLength(1024)
            .Destructure.ToMaximumCollectionCount(32)
            .WriteTo.Debug()
            .WriteTo.Console();

        Log.Logger = loggerConfig.CreateLogger();
    }

    [OneTimeTearDown]
    public void End()
    {
        Log.CloseAndFlush();
    }
}
