using System;
using InSync.Counter.Control;
using InSync.Counter.SensorData;
using InSync.SensorDataStorage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Config;
using NLog.Extensions.Logging;

namespace InSync.Counter
{
    public class CounterStartup
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

            // sensor data
            services.AddScoped<ISensorDataReceiver, SensorDataReceiver>();
            services.AddScoped<ISensorData, InMemorySensorData>();
            services.AddScoped<ISensorDataProcessor, SensorDataProcessor>();
            services.AddSingleton<ISensorDataWriter, SensorDataWriter>();
            services.AddSingleton<ISensorDataUtil, SensorDataUtil>();
            services.Configure<SensorDataOptions>(configuration.GetSection("SensorData"));

            // control
            services.AddScoped<IControlReceiver, ControlReceiver>();
            services.Configure<ControlOptions>(configuration.GetSection("Control"));

            IServiceProvider serviceProvider = services.BuildServiceProvider();
            return serviceProvider;
        }
    }
}
