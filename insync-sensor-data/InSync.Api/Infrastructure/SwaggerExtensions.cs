using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace InSync.Api.Infrastructure
{
    public sealed class SwaggerOptions
    {
        public bool IsEnabled { get; set; }
        public string Url { get; set; }
        public OpenApiInfo OpenApi { get; set; }
    }

    public static class SwaggerExtensions
    {
        public static void AddSwagger(this IServiceCollection services, SwaggerOptions options)
        {
            if (options.IsEnabled)
            {
                services.AddSwaggerGen(c =>
                {
                    c.SwaggerDoc(options.OpenApi.Version, new OpenApiInfo
                    {
                        Version = options.OpenApi.Version,
                        Title = options.OpenApi.Title,
                        Description = options.OpenApi.Description
                    });
                });
                services.ConfigureSwaggerGen(swaggerGen => swaggerGen.CustomSchemaIds(type => type.FullName));
            }
        }

        public static void UseSwagger(this IApplicationBuilder app, SwaggerOptions options)
        {
            if (options.IsEnabled)
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint(options.Url, name: $"{options.OpenApi.Title} - {options.OpenApi.Version}");
                });
            }
        }
    }
}
