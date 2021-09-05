using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Web;

namespace InSync.Api
{
    public static class ApiProgram
    {
        public static void Main(string[] args)
        {
            LogManager.LoadConfiguration("NLog.config");
            Logger logger = LogManager.GetLogger("Main");

            try
            {
                IHostBuilder hostBuilder = CreateHostBuilder(args);
                IHost host = hostBuilder.Build();
                host.Run();
            }
            catch (Exception exception)
            {
                logger.Fatal(exception, "Failed to start.");
            }
            finally
            {
                LogManager.Shutdown();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host
                .CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<ApiStartup>();
                })
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                })
                .UseNLog();
        }
    }
}
