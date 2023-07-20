using OpenTelemetry;

namespace Common.Jaeger;

public sealed class JaegerOptions
{
    public string AgentHost { get; init; } = string.Empty;

    public int AgentPort { get; init; }

    public ExportProcessorType ExportProcessorType { get; init; }

    public int BatchExportScheduledDelayMillis { get; init; }
}
