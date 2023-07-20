using Microsoft.OpenApi.Models;

namespace Common.AspNet.Swagger;

public sealed class SwaggerOptions
{
    public bool UseSwagger { get; init; }

    public bool UseSwaggerUi { get; init; }

    public string Url { get; init; } = string.Empty;

    public bool AddAuthorizationHeader { get; init; }

    public OpenApiInfo OpenApiInfo { get; init; } = new();
}
