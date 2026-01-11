namespace Xavier.Options;

/// <summary>
/// Options for Correlation ID feature.
/// </summary>
public sealed class CorrelationOptions
{
    /// <summary>
    /// Whether correlation ID feature is enabled. Default: true.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// The header name for correlation ID. Default: "X-Correlation-Id".
    /// </summary>
    public string HeaderName { get; set; } = XavierDefaults.Correlation.HeaderName;

    /// <summary>
    /// Whether to add correlation ID to response headers. Default: true.
    /// </summary>
    public bool AddToResponse { get; set; } = true;

    /// <summary>
    /// Whether to parse W3C traceparent header for correlation ID. Default: false.
    /// Note: Setting this to true will use the traceparent header's trace ID as correlation ID if present.
    /// </summary>
    public bool UseTraceparent { get; set; }

    /// <summary>
    /// Whether to enrich log scopes with correlation data. Default: true.
    /// </summary>
    public bool EnrichLogScopes { get; set; } = true;
}
