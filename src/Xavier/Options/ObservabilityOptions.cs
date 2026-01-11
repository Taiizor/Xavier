using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Xavier.Options;

/// <summary>
/// Options for OpenTelemetry observability feature.
/// </summary>
public sealed class ObservabilityOptions
{
    /// <summary>
    /// Whether observability is enabled. Default: false (opt-in).
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// The service name for telemetry. Required when enabled.
    /// </summary>
    public string? ServiceName { get; set; }

    /// <summary>
    /// The service version for telemetry.
    /// </summary>
    public string? ServiceVersion { get; set; }

    /// <summary>
    /// Whether to enable tracing. Default: true (when Enabled).
    /// </summary>
    public bool EnableTracing { get; set; } = true;

    /// <summary>
    /// Whether to enable metrics. Default: true (when Enabled).
    /// </summary>
    public bool EnableMetrics { get; set; } = true;

    /// <summary>
    /// Whether to enable runtime metrics (GC, threads, etc.). Default: true (when Enabled).
    /// </summary>
    public bool EnableRuntimeMetrics { get; set; } = true;

    /// <summary>
    /// Callback to configure additional tracing options including exporters.
    /// Note: Xavier does NOT auto-enable exporters. Use this callback to add exporters.
    /// Example: opts.ConfigureTracing = tracing => tracing.AddOtlpExporter();
    /// </summary>
    public Action<TracerProviderBuilder>? ConfigureTracing { get; set; }

    /// <summary>
    /// Callback to configure additional metrics options including exporters.
    /// Note: Xavier does NOT auto-enable exporters. Use this callback to add exporters.
    /// Example: opts.ConfigureMetrics = metrics => metrics.AddOtlpExporter();
    /// </summary>
    public Action<MeterProviderBuilder>? ConfigureMetrics { get; set; }
}
