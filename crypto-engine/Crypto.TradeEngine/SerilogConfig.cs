using System.Reflection;
using Serilog;
using Serilog.Events;

namespace Crypto.TradeEngine;

public static class SerilogConfig
{
    public static void Setup(Assembly entryAssembly, string environment)
    {
        LoggerConfiguration configuration = CreateDefaultConfiguration(entryAssembly);

        switch (environment.ToLowerInvariant())
        {
            case "development":
                configuration = ApplyDevelopmentConfiguration(configuration);
                break;
            case "production":
                configuration = ApplyProductionConfiguration(configuration);
                break;
            default:
                throw new InvalidOperationException($"Logging configuration doesn't support environment '{environment}'.");
        }

        Log.Logger = configuration.CreateLogger();
    }

    private static LoggerConfiguration CreateDefaultConfiguration(Assembly entryAssembly)
    {
        return new LoggerConfiguration()
            .MinimumLevel.Is(LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .Enrich.WithMachineName()
            .Enrich.WithProperty("Application", entryAssembly.GetName().Name!)
            .Enrich.FromLogContext()
            .Destructure.ToMaximumDepth(4)
            .Destructure.ToMaximumStringLength(1024)
            .Destructure.ToMaximumCollectionCount(32);
    }

    private static LoggerConfiguration ApplyDevelopmentConfiguration(LoggerConfiguration configuration)
    {
        return configuration
            .MinimumLevel.Override("Crypto", LogEventLevel.Verbose)
            .WriteTo.Console();
    }

    private static LoggerConfiguration ApplyProductionConfiguration(LoggerConfiguration configuration)
    {
        return configuration
            // TODO: use JSON formatters in order for log aggregators to collect logs
            .WriteTo.Console();
    }
}
