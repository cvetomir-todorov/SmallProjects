using Common.Testing;
using HandGame.Api;
using HandGame.Api.Random;
using HandGame.Testing.Api.Infra;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using RestSharp;
using Serilog;

namespace HandGame.Testing.Api.Tests;

public abstract class Base
{
    // will not to be null because NUnit calls the OneTimeSetUp method which initializes them
    protected AspNetApplication App { get; private set; } = null!;
    protected RestClient Client { get; private set; } = null!;

    [OneTimeSetUp]
    public void OnceBeforeAllTests()
    {
        App = new AspNetApplication(typeof(Startup));
        App.ConfigureWebHost(webHost =>
        {
            webHost.UseUrls("http://localhost:17777");
        });
        App.ConfigureAppConfiguration(config =>
        {
            string path = Path.Combine(TestContext.CurrentContext.TestDirectory, "test-appsettings.json");
            config.AddJsonFile(path, optional: false);
        });
        App.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddSerilog(dispose: true);
        });
        App.ConfigureServices(services =>
        {
            services.AddSingleton<IRandom, StubRandom>();
        });

        App.Start();
        Client = new RestClient(App.CreateHttpClient());
    }

    [OneTimeTearDown]
    public void OnceAfterAllTests()
    {
        Client.Dispose();
        App.Dispose();
    }
}
