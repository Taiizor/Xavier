namespace Xavier.Options;

/// <summary>
/// Options for HTTP logging feature.
/// </summary>
public sealed class HttpLoggingOptions
{
    /// <summary>
    /// Whether HTTP logging is enabled. Default: false (opt-in).
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Whether to log request headers. Default: true.
    /// </summary>
    public bool LogRequestHeaders { get; set; } = true;

    /// <summary>
    /// Whether to log response headers. Default: true.
    /// </summary>
    public bool LogResponseHeaders { get; set; } = true;

    /// <summary>
    /// Whether to log request body. Default: false (security).
    /// </summary>
    public bool LogRequestBody { get; set; }

    /// <summary>
    /// Whether to log response body. Default: false (security).
    /// </summary>
    public bool LogResponseBody { get; set; }

    /// <summary>
    /// Headers to exclude from logging. Defaults include Authorization, Cookie, etc.
    /// </summary>
    public List<string> ExcludedHeaders { get; set; } = [.. XavierDefaults.HttpLogging.ExcludedHeaders];

    /// <summary>
    /// Request body size limit in bytes. Default: 32KB.
    /// </summary>
    public int RequestBodyLogLimit { get; set; } = 32 * 1024;

    /// <summary>
    /// Response body size limit in bytes. Default: 32KB.
    /// </summary>
    public int ResponseBodyLogLimit { get; set; } = 32 * 1024;
}
