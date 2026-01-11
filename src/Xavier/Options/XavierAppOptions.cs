namespace Xavier.Options;

/// <summary>
/// Options for UseXavier() application builder configuration.
/// </summary>
public sealed class XavierAppOptions
{
    /// <summary>
    /// Whether to use correlation ID middleware. Default: true.
    /// </summary>
    public bool UseCorrelation { get; set; } = true;

    /// <summary>
    /// Whether to use error handling middleware. Default: true.
    /// </summary>
    public bool UseErrorHandling { get; set; } = true;

    /// <summary>
    /// Whether to use rate limiting middleware. Default: true (if enabled in XavierOptions).
    /// </summary>
    public bool UseRateLimiting { get; set; } = true;

    /// <summary>
    /// Whether to use HTTP logging middleware. Default: true (if enabled in XavierOptions).
    /// </summary>
    public bool UseHttpLogging { get; set; } = true;
}
