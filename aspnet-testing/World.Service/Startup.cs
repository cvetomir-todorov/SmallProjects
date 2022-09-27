using System.Reflection;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using World.Service.Database;
using World.Service.Database.Continents;
using World.Service.Database.Countries;
using World.Service.Endpoints.Continents;

namespace World.Service;

public class Startup
{
    private readonly IConfiguration _configuration;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddSwaggerGen();
        services.AddFluentValidationAutoValidation(fluentValidation =>
        {
            fluentValidation.DisableDataAnnotationsValidation = true;
        });
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
        services.AddDbContext<WorldDbContext>(ef =>
        {
            ef.UseInMemoryDatabase(databaseName: "World");
        });
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        services.AddScoped<IContinentRepo, ContinentRepo>();
        services.AddScoped<ICountryRepo, CountryRepo>();
        services.Configure<ContinentOptions>(_configuration.GetSection("Continents"));
    }

    public void Configure(IApplicationBuilder app, IHostEnvironment env)
    {
        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAuthorization();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });

        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
    }
}
