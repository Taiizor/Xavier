namespace Xavier.Options;

/// <summary>
/// Root options class for Xavier configuration.
/// Binds from the "Xavier" configuration section.
/// </summary>
public sealed class XavierOptions
{
    /// <summary>
    /// Error handling and ProblemDetails options. Default: Enabled.
    /// </summary>
    public ErrorHandlingOptions ErrorHandling { get; set; } = new();

    /// <summary>
    /// Correlation ID options. Default: Enabled.
    /// </summary>
    public CorrelationOptions Correlation { get; set; } = new();

    /// <summary>
    /// HTTP logging options. Default: Disabled.
    /// </summary>
    public HttpLoggingOptions HttpLogging { get; set; } = new();

    /// <summary>
    /// Rate limiting options. Default: Disabled.
    /// </summary>
    public RateLimitingOptions RateLimiting { get; set; } = new();

    /// <summary>
    /// OpenAPI options. Default: Enabled on net8.0+.
    /// </summary>
    public OpenApiOptions OpenApi { get; set; } = new();

    /// <summary>
    /// Health endpoint options. Default: Enabled.
    /// </summary>
    public HealthOptions Health { get; set; } = new();

    /// <summary>
    /// HTTP client resilience options. Default: Disabled.
    /// </summary>
    public ResilienceOptions Resilience { get; set; } = new();

    /// <summary>
    /// OpenTelemetry observability options. Default: Disabled.
    /// </summary>
    public ObservabilityOptions Observability { get; set; } = new();
}
