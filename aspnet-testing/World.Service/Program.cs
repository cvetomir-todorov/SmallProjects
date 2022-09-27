using Serilog;
using Serilog.Events;

namespace World.Service;

public static class Program
{
    public static void Main(params string[] args)
    {
        SetupSerilog();
        CreateWebHostBuilder(args).Build().Run();
    }

    private static void SetupSerilog()
    {
        LoggerConfiguration loggerConfig = new LoggerConfiguration()
            .MinimumLevel.Is(LogEventLevel.Verbose)
            .MinimumLevel.Override("System.Net.Http.HttpClient", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .Destructure.ToMaximumDepth(8)
            .Destructure.ToMaximumStringLength(1024)
            .Destructure.ToMaximumCollectionCount(32)
            .WriteTo.Console();

        Log.Logger = loggerConfig.CreateLogger();
    }

    private static IHostBuilder CreateWebHostBuilder(string[] args)
    {
        return Host
            .CreateDefaultBuilder()
            .ConfigureWebHostDefaults(webHostBuilder =>
            {
                webHostBuilder.UseStartup<Startup>();
            })
            .ConfigureLogging(loggingBuilder =>
            {
                loggingBuilder.ClearProviders();
                loggingBuilder.AddSerilog(dispose: true);
            })
            .ConfigureAppConfiguration(configBuilder =>
            {
                configBuilder.AddCommandLine(args);
            });
    }
}
