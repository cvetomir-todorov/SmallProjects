using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Common.Testing;

public class AspNetApplication : IDisposable
{
    private readonly Type _startupType;
    private readonly string _applicationName;
    private readonly string _httpClientName;
    private Func<IHostBuilder> _createDefaultHostBuilder;
    private Action<IWebHostBuilder>? _configureWebHost;
    private Action<IServiceCollection>? _configureServices;
    private Action<ILoggingBuilder>? _configureLogging;
    private Action<IConfigurationBuilder>? _configureAppConfiguration;
    private Uri? _baseUrl;
    private IHost? _host;

    public AspNetApplication(Type startupType, string? applicationName = null, string? httpClientName = null)
    {
        _startupType = startupType ?? throw new ArgumentNullException(nameof(startupType));

        applicationName ??= startupType.FullName;
        if (string.IsNullOrWhiteSpace(applicationName))
        {
            throw new ArgumentException($"Argument {nameof(applicationName)} should not be empty or whitespace.", nameof(applicationName));
        }
        _applicationName = applicationName;

        httpClientName ??= startupType.FullName;
        if (string.IsNullOrWhiteSpace(httpClientName))
        {
            throw new ArgumentException($"Argument {nameof(httpClientName)} should not be empty or whitespace.", nameof(httpClientName));
        }
        _httpClientName = httpClientName;

        _createDefaultHostBuilder = Host.CreateDefaultBuilder;
    }

    public void Dispose()
    {
        CleanUp(isDisposing: true);
        GC.SuppressFinalize(this);
    }

    protected virtual void CleanUp(bool isDisposing)
    {
        if (isDisposing)
        {
            _host?.Dispose();
        }
    }

    public void ConfigureCreationOfDefaultHostBuilder(Func<IHostBuilder> createDefaultHostBuilder)
    {
        _createDefaultHostBuilder = createDefaultHostBuilder ?? throw new ArgumentNullException(nameof(createDefaultHostBuilder));
    }

    public void ConfigureWebHost(Action<IWebHostBuilder> configureWebHost)
    {
        _configureWebHost = configureWebHost ?? throw new ArgumentNullException(nameof(configureWebHost));
    }

    public void ConfigureServices(Action<IServiceCollection> configureServices)
    {
        _configureServices = configureServices ?? throw new ArgumentNullException(nameof(configureServices));
    }

    public void ConfigureLogging(Action<ILoggingBuilder> configureLogging)
    {
        _configureLogging = configureLogging ?? throw new ArgumentNullException(nameof(configureLogging));
    }

    public void ConfigureAppConfiguration(Action<IConfigurationBuilder> configureAppConfiguration)
    {
        _configureAppConfiguration = configureAppConfiguration ?? throw new ArgumentNullException(nameof(configureAppConfiguration));
    }

    public void Start(CancellationToken ct = default)
    {
        if (_host != null)
        {
            throw new InvalidOperationException($"ASP.NET application {_applicationName} cannot be started - it is already running.");
        }

        IHostBuilder hostBuilder = _createDefaultHostBuilder.Invoke();
        ConfigureWebHost(hostBuilder);
        ConfigureHost(hostBuilder);

        _host = hostBuilder.Build();
        _host
            .RunAsync(ct)
            .ContinueWith(task => Console.WriteLine(task.Exception), TaskContinuationOptions.OnlyOnFaulted);
    }

    private void ConfigureWebHost(IHostBuilder hostBuilder)
    {
        hostBuilder.ConfigureWebHost(webHostBuilder =>
        {
            string? assemblyName = _startupType.Assembly.GetName().Name;
            if (assemblyName == null)
            {
                throw new InvalidOperationException($"Unable to obtain project name for ASP.NET application '{_applicationName}'.");
            }

            SetSolutionRelativeContentRoot(webHostBuilder, assemblyName);
            webHostBuilder.UseStartup(_startupType);
            webHostBuilder.UseKestrel();
            _configureWebHost?.Invoke(webHostBuilder);

            webHostBuilder.ConfigureServices(services =>
            {
                // dedicated HTTP test client
                services.AddHttpClient(_httpClientName, httpClient =>
                    {
                        httpClient.BaseAddress = _baseUrl;
                        httpClient.DefaultRequestHeaders.Add("User-agent", "aspnet-test-client");
                    })
                    .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler());

                _configureServices?.Invoke(services);
            });

            _baseUrl = GetBaseUrl(webHostBuilder);
        });
    }

    private static void SetSolutionRelativeContentRoot(IWebHostBuilder webHostBuilder, string assemblyName)
    {
        string applicationBasePath = AppContext.BaseDirectory;
        DirectoryInfo? directoryInfo = new(applicationBasePath);
        do
        {
            string? solutionPath = Directory.EnumerateFiles(directoryInfo.FullName, "*.sln").FirstOrDefault();
            if (solutionPath != null)
            {
                string contentRoot = Path.GetFullPath(Path.Combine(directoryInfo.FullName, assemblyName));
                webHostBuilder.UseContentRoot(contentRoot);
                return;
            }

            directoryInfo = directoryInfo.Parent;
        }
        while (directoryInfo != null);

        throw new InvalidOperationException($"Solution root could not be located using application base path {applicationBasePath}.");
    }

    private Uri GetBaseUrl(IWebHostBuilder webHostBuilder)
    {
        string? serverUrlsKey = webHostBuilder.GetSetting(WebHostDefaults.ServerUrlsKey);
        if (string.IsNullOrWhiteSpace(serverUrlsKey))
        {
            string useUrlsMethod = nameof(HostingAbstractionsWebHostBuilderExtensions.UseUrls);
            throw new InvalidOperationException($"Set the server URLs via '{useUrlsMethod}' extension method for ASP.NET application {_applicationName}.");
        }

        string[] serverUrls = serverUrlsKey.Split(';', StringSplitOptions.RemoveEmptyEntries);
        if (serverUrls == null || serverUrls.Length == 0)
        {
            throw new InvalidOperationException($"There are no valid server URLs set for ASP.NET application {_applicationName}.");
        }
        if (serverUrls.Length > 1)
        {
            throw new InvalidOperationException($"There are multiple server URLs set for ASP.NET application {_applicationName}.");
        }

        return new Uri(serverUrls.First());
    }

    private void ConfigureHost(IHostBuilder hostBuilder)
    {
        hostBuilder.ConfigureAppConfiguration(configurationBuilder =>
        {
            _configureAppConfiguration?.Invoke(configurationBuilder);
        });
        hostBuilder.ConfigureLogging(logging =>
        {
            if (_configureLogging != null)
            {
                _configureLogging.Invoke(logging);
            }
            else
            {
                logging.AddDebug();
                logging.AddConsole();
            }
        });
    }

    public IServiceProvider ServiceProvider
    {
        get
        {
            VerifyStarted();
            return _host!.Services;
        }
    }

    public HttpClient CreateHttpClient()
    {
        VerifyStarted();

        try
        {
            IHttpClientFactory factory = _host!.Services.GetRequiredService<IHttpClientFactory>();
            HttpClient client = factory.CreateClient(_httpClientName);

            return client;
        }
        catch (ObjectDisposedException exception)
        {
            throw new InvalidOperationException($"ASP.NET application {_applicationName} has stopped.", exception);
        }
    }

    public Uri BaseUrl
    {
        get
        {
            VerifyStarted();
            return _baseUrl!;
        }
    }

    private void VerifyStarted()
    {
        if (_host == null)
        {
            throw new InvalidOperationException($"ASP.NET application {_applicationName} is not started yet.");
        }
    }
}
