namespace Xavier.Options;

/// <summary>
/// Options for HTTP client resilience feature.
/// </summary>
public sealed class ResilienceOptions
{
    /// <summary>
    /// Whether resilience is enabled. Default: false (opt-in).
    /// Note: Standard resilience requires .NET 8 or later.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Whether to use standard resilience handler. Default: true (when Enabled).
    /// </summary>
    public bool UseStandardResilience { get; set; } = true;

    /// <summary>
    /// Maximum retry attempts. Default: 3.
    /// </summary>
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Timeout in seconds for individual requests. Default: 30.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;
}
