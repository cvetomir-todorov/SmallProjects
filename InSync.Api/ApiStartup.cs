using InSync.Api.Counter;
using InSync.Api.Infrastructure;
using InSync.Api.Sensor;
using InSync.SensorDataStorage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace InSync.Api
{
    public class ApiStartup
    {
        public ApiStartup(IConfiguration configuration)
        {
            Configuration = configuration;
            SwaggerOptions = new SwaggerOptions();
            Configuration.GetSection("Swagger").Bind(SwaggerOptions);
        }

        public IConfiguration Configuration { get; }

        private SwaggerOptions SwaggerOptions { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSwagger(SwaggerOptions);

            services.AddSingleton<ISensorDataReader, SensorDataReader>();
            services.AddSingleton<ISensorDataUtil, SensorDataUtil>();
            services.AddSingleton<ICounterClient, CounterClient>();
            services.AddSingleton<IMemoryCache, MemoryCache>();
            services.Configure<CounterOptions>(Configuration.GetSection("Counter"));
            services.Configure<SensorOptions>(Configuration.GetSection("Sensor"));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseSwagger(SwaggerOptions);
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            ICounterClient counterClient = app.ApplicationServices.GetRequiredService<ICounterClient>();
            counterClient.Start();
        }
    }
}
