using System.Net;
using System.Reflection;
using AspNetCoreRateLimit;
using Common.AspNet.Health;
using Common.AspNet.Prometheus;
using Common.AspNet.Swagger;
using Common.Http.Health;
using Common.Jaeger;
using Common.Otel;
using FluentValidation;
using FluentValidation.AspNetCore;
using HandGame.Api.Endpoints;
using HandGame.Api.Game;
using HandGame.Api.Infra;
using HandGame.Api.Random;
using HandGame.Api.Telemetry;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Polly;
using Polly.Timeout;

namespace HandGame.Api;

public class Startup
{
    private readonly IConfiguration _configuration;
    private readonly RandomOptions _randomOptions;
    private readonly SwaggerOptions _swaggerOptions;
    private readonly OtelSamplingOptions _otelSamplingOptions;
    private readonly JaegerOptions _jaegerOptions;
    private readonly PrometheusOptions _prometheusOptions;

    public Startup(IConfiguration configuration)
    {
        _configuration = configuration;

        _randomOptions = new();
        _configuration.GetSection("Random").Bind(_randomOptions);

        _swaggerOptions = new();
        _configuration.GetSection("Swagger").Bind(_swaggerOptions);

        _otelSamplingOptions = new();
        _configuration.GetSection("OtelSampling").Bind(_otelSamplingOptions);

        _jaegerOptions = new();
        _configuration.GetSection("Jaeger").Bind(_jaegerOptions);

        _prometheusOptions = new();
        _configuration.GetSection("Prometheus").Bind(_prometheusOptions);
    }

    public void ConfigureServices(IServiceCollection services)
    {
        ConfigureEndpointServices(services);
        ConfigureGameServices(services);
        ConfigureRandomGeneratorServices(services);
        ConfigureRateLimiting(services);
        ConfigureHealthChecks(services);
        ConfigureTelemetry(services);
    }

    private void ConfigureEndpointServices(IServiceCollection services)
    {
        services.AddControllers();
        services.AddOutputCache();
        services.AddSwaggerServices(_swaggerOptions);

        services.AddFluentValidationAutoValidation(fluentValidation =>
        {
            fluentValidation.DisableDataAnnotationsValidation = true;
        });
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddSingleton<ChoiceDtoFlyweight>();
    }

    private static void ConfigureGameServices(IServiceCollection services)
    {
        services.AddSingleton<GameEngine>();
        services.AddSingleton<IGameInfo>(serviceProvider => serviceProvider.GetRequiredService<GameEngine>());
        services.AddSingleton<IGame>(serviceProvider => serviceProvider.GetRequiredService<GameEngine>());
        services.AddSingleton<GameTelemetry>();
    }

    private void ConfigureRandomGeneratorServices(IServiceCollection services)
    {
        services.AddSingleton<IRandom, ExternalRandom>();
        services.Configure<RandomOptions>(_configuration.GetSection("Random"));

        int attemptSeconds = Convert.ToInt32(Math.Ceiling(_randomOptions.AttemptTimeout.TotalSeconds));
        IAsyncPolicy<HttpResponseMessage> attemptTimeoutPolicy = Policy
            .TimeoutAsync<HttpResponseMessage>(attemptSeconds);

        IAsyncPolicy<HttpResponseMessage> handleAndRetryPolicy = Policy
            .HandleResult<HttpResponseMessage>(response =>
            {
                // ignore redirects 3xx and most client errors 4xx
                // retry in presence of rate limiters and server errors
                bool shouldRetry = response.StatusCode == HttpStatusCode.TooManyRequests || (int)response.StatusCode >= 500;
                return shouldRetry;
            })
            // handle the timeout exception thrown by polly
            .Or<TimeoutRejectedException>()
            .Or<HttpRequestException>()
            .WaitAndRetryAsync(_randomOptions.SleepIntervals);

        services
            .AddHttpClient("external-random")
            // there is no data about expected load and how many connections the external random generator can handle
            // but we can configure some of these using a custom primary HTTP message handler
            // .ConfigurePrimaryHttpMessageHandler(() => new SocketsHttpHandler
            // {
            //     MaxConnectionsPerServer = 10
            // })
            .AddPolicyHandler(handleAndRetryPolicy)
            // the attempt-timeout-policy absolutely needs to be placed within the handle-and-retry-policy
            .AddPolicyHandler(attemptTimeoutPolicy);
    }

    private void ConfigureRateLimiting(IServiceCollection services)
    {
        services.AddMemoryCache();
        services.Configure<IpRateLimitOptions>(_configuration.GetSection("IpRateLimiting"));
        services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
        services.AddInMemoryRateLimiting();
    }

    private void ConfigureHealthChecks(IServiceCollection services)
    {
        services
            .AddHealthChecks()
            .AddUri(
                name: "random",
                uri: new Uri(_randomOptions.HealthAddress),
                timeout: _randomOptions.HealthTimeout,
                tags: new[] { HealthTags.Health, HealthTags.Ready });
    }

    private void ConfigureTelemetry(IServiceCollection services)
    {
        ResourceBuilder serviceResourceBuilder = ResourceBuilder
            .CreateEmpty()
            .AddService(serviceName: "HandGame.Api", serviceVersion: "0.1")
            .AddEnvironmentVariableDetector();

        services
            .AddOpenTelemetry()
            .WithTracing(tracing =>
            {
                tracing.SetResourceBuilder(serviceResourceBuilder);
                tracing.AddAspNetCoreInstrumentation(aspnet =>
                {
                    HashSet<string> excludedPaths = new()
                    {
                        _prometheusOptions.ScrapeEndpointPath, HealthPaths.Health, HealthPaths.Startup, HealthPaths.Live, HealthPaths.Ready
                    };
                    aspnet.Filter = httpContext => !excludedPaths.Contains(httpContext.Request.Path) && !httpContext.Request.Path.StartsWithSegments("/swagger");
                });
                tracing.AddHttpClientInstrumentation();
                tracing.ConfigureSampling(_otelSamplingOptions);
                tracing.ConfigureJaegerExporter(_jaegerOptions);
            })
            .WithMetrics(metrics =>
            {
                metrics.SetResourceBuilder(serviceResourceBuilder);
                metrics.AddAspNetCoreInstrumentation();
                metrics.AddHttpClientInstrumentation();
                metrics.AddGameInstrumentation();
                metrics.ConfigurePrometheusAspNetExporter(_prometheusOptions);
            });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        app.UseCustomExceptionHandler();
        app.UseHttpsRedirection();

        app.UseIpRateLimiting();
        app.UseRouting();
        app.UseOutputCache();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapHttpHealthEndpoints(setup =>
            {
                setup.Startup.Enabled = false;

                Func<HttpContext, HealthReport, Task> responseWriter = (context, report) => CustomHealth.Writer(serviceName: "handgame-api", context, report);
                setup.Health.ResponseWriter = responseWriter;

                if (env.IsDevelopment())
                {
                    setup.Live.ResponseWriter = responseWriter;
                    setup.Ready.ResponseWriter = responseWriter;
                }
            });
        });

        app.UseOpenTelemetryPrometheusScrapingEndpoint(context => context.Request.Path == _prometheusOptions.ScrapeEndpointPath);

        app.MapWhen(context => context.Request.Path.StartsWithSegments("/swagger"), _ =>
        {
            app.UseSwaggerMiddlewares(_swaggerOptions);
        });
    }
}
