using System;
using InSync.Sensor.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Config;
using NLog.Extensions.Logging;

namespace InSync.Sensor
{
    public class SensorStartup
    {
        public IServiceProvider ConfigureServices()
        {
            ServiceCollection services = new ServiceCollection();

            services.AddLogging(logging =>
            {
                logging.ClearProviders();
                logging.AddNLog(new XmlLoggingConfiguration("NLog.config"));
            });

            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            services.AddScoped<ISensorDataSender, SensorDataSender>();
            services.Configure<SensorDataSenderOptions>(configuration.GetSection("Sender"));

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            return serviceProvider;
        }
    }
}
