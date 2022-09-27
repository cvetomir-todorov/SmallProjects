using Common.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using RestSharp;
using Serilog;
using World.Service;
using World.Service.Database;
using World.Service.Database.Continents;
using World.Service.Database.Countries;

namespace World.Testing.Service.Tests;

public abstract class BaseTest
{
    protected AspNetApplication Application { get; private set; } = null!;
    protected RestClient RestClient { get; private set; } = null!;
    protected WorldDbContext DbContext { get; private set; } = null!;

    [OneTimeSetUp]
    public void Begin()
    {
        InitAspNetApplication();
        Application.Start();

        RestClient = new RestClient(Application.CreateHttpClient());
        DbContext = new WorldDbContext(new DbContextOptionsBuilder<WorldDbContext>()
            .UseInMemoryDatabase("World")
            .Options);

        AddTestData();
    }

    private void InitAspNetApplication()
    {
        Application = new AspNetApplication(typeof(Startup));

        Application.ConfigureWebHost(webHostBuilder =>
        {
            webHostBuilder.UseUrls("http://localhost:20013");
        });
        Application.ConfigureServices(services =>
        {
            // override service registrations
        });
        Application.ConfigureLogging(logging =>
        {
            logging.AddSerilog(dispose: true);
        });
        Application.ConfigureAppConfiguration(config =>
        {
            string path = Path.Combine(TestContext.CurrentContext.TestDirectory, "test-appsettings.json");
            config.AddJsonFile(path, optional: false);
        });
    }

    protected virtual void AddTestData()
    { }

    [OneTimeTearDown]
    public void End()
    {
        Application.Dispose();
        CleanUpDb();
        DbContext.Dispose();
    }

    private void CleanUpDb()
    {
        List<ContinentEntity> continents = DbContext.Continents.ToList();
        List<CountryEntity> countries = DbContext.Countries.ToList();

        DbContext.Continents.RemoveRange(continents);
        DbContext.Countries.RemoveRange(countries);

        DbContext.SaveChanges();
    }
}
