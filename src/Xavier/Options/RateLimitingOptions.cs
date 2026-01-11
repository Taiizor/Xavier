namespace Xavier.Options;

/// <summary>
/// Options for rate limiting feature.
/// </summary>
public sealed class RateLimitingOptions
{
    /// <summary>
    /// Whether rate limiting is enabled. Default: false (opt-in).
    /// Note: Rate limiting requires .NET 7 or later.
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Whether to enable global rate limiting policy. Default: true (when Enabled).
    /// </summary>
    public bool EnableGlobalPolicy { get; set; } = true;

    /// <summary>
    /// Whether to enable per-IP rate limiting policy. Default: true (when Enabled).
    /// </summary>
    public bool EnablePerIpPolicy { get; set; } = true;

    /// <summary>
    /// Whether to enable per-client rate limiting policy. Default: false.
    /// </summary>
    public bool EnablePerClientPolicy { get; set; }

    /// <summary>
    /// The header name for client ID. Default: "X-Client-Id".
    /// </summary>
    public string ClientIdHeader { get; set; } = XavierDefaults.RateLimiting.ClientIdHeader;

    /// <summary>
    /// The permit limit for rate limiting. Default: 100.
    /// </summary>
    public int PermitLimit { get; set; } = XavierDefaults.RateLimiting.DefaultPermitLimit;

    /// <summary>
    /// The window duration in seconds. Default: 60.
    /// </summary>
    public int WindowSeconds { get; set; } = XavierDefaults.RateLimiting.DefaultWindowSeconds;

    /// <summary>
    /// The queue limit for requests that exceed the permit limit. Default: 0 (no queuing).
    /// </summary>
    public int QueueLimit { get; set; }

    /// <summary>
    /// Whether to add rate limit headers to responses. Default: true.
    /// Headers: X-RateLimit-Limit, X-RateLimit-Remaining, X-RateLimit-Reset, Retry-After
    /// </summary>
    public bool AddRateLimitHeaders { get; set; } = true;
}
