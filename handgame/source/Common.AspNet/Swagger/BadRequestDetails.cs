namespace Common.AspNet.Swagger;

/// <summary>
/// Used in Swagger documentation.
/// </summary>
public sealed class BadRequestDetails
{
    public string Type { get; init; } = string.Empty;

    public string Title { get; init; } = string.Empty;

    public int Status { get; init; }

    public string TraceId { get; init; } = string.Empty;

    public Dictionary<string, string[]> Errors { get; init; } = new();
}
