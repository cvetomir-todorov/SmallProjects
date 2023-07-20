using OpenTelemetry.Trace;

namespace Common.Jaeger;

public static class JaegerRegistrations
{
    public static TracerProviderBuilder ConfigureJaegerExporter(this TracerProviderBuilder tracing, JaegerOptions options)
    {
        return tracing
            .AddJaegerExporter(jaeger =>
            {
                jaeger.AgentHost = options.AgentHost;
                jaeger.AgentPort = options.AgentPort;
                jaeger.ExportProcessorType = options.ExportProcessorType;
                jaeger.BatchExportProcessorOptions.ScheduledDelayMilliseconds = options.BatchExportScheduledDelayMillis;
            });
    }
}
