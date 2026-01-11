namespace Xavier.Options;

/// <summary>
/// Options for error handling and ProblemDetails configuration.
/// </summary>
public sealed class ErrorHandlingOptions
{
    /// <summary>
    /// Whether error handling is enabled. Default: true.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Whether to include exception details in error responses.
    /// Should only be true in development environments.
    /// Default: false.
    /// </summary>
    public bool IncludeExceptionDetails { get; set; }

    /// <summary>
    /// Whether to include trace ID in ProblemDetails. Default: true.
    /// </summary>
    public bool IncludeTraceId { get; set; } = true;

    /// <summary>
    /// Whether to include correlation ID in ProblemDetails. Default: true.
    /// </summary>
    public bool IncludeCorrelationId { get; set; } = true;
}
