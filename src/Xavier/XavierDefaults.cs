namespace Xavier;

/// <summary>
/// Contains default constant values used throughout Xavier.
/// </summary>
public static class XavierDefaults
{
    /// <summary>
    /// The default configuration section name for Xavier options.
    /// </summary>
    public const string ConfigurationSection = "Xavier";

    /// <summary>
    /// Default values for Correlation ID feature.
    /// </summary>
    public static class Correlation
    {
        /// <summary>
        /// The default header name for correlation ID.
        /// </summary>
        public const string HeaderName = "X-Correlation-Id";

        /// <summary>
        /// The W3C traceparent header name.
        /// </summary>
        public const string TraceparentHeader = "traceparent";
    }

    /// <summary>
    /// Default values for Health endpoints.
    /// </summary>
    public static class Health
    {
        /// <summary>
        /// The default path for liveness health check.
        /// </summary>
        public const string LivenessPath = "/health";

        /// <summary>
        /// The default path for readiness health check.
        /// </summary>
        public const string ReadinessPath = "/ready";

        /// <summary>
        /// The tag used to identify readiness health checks.
        /// </summary>
        public const string ReadinessTag = "ready";
    }

    /// <summary>
    /// Default values for OpenAPI feature.
    /// </summary>
    public static class OpenApi
    {
        /// <summary>
        /// The default document name.
        /// </summary>
        public const string DocumentName = "v1";

        /// <summary>
        /// The default path template for OpenAPI document.
        /// </summary>
        public const string PathTemplate = "/openapi/{documentName}.json";
    }

    /// <summary>
    /// Default values for Rate Limiting feature.
    /// </summary>
    public static class RateLimiting
    {
        /// <summary>
        /// The name of the global rate limit policy.
        /// </summary>
        public const string GlobalPolicyName = "Xavier.Global";

        /// <summary>
        /// The name of the per-IP rate limit policy.
        /// </summary>
        public const string PerIpPolicyName = "Xavier.PerIP";

        /// <summary>
        /// The name of the per-client rate limit policy.
        /// </summary>
        public const string PerClientPolicyName = "Xavier.PerClient";

        /// <summary>
        /// The default header name for client ID.
        /// </summary>
        public const string ClientIdHeader = "X-Client-Id";

        /// <summary>
        /// The default permit limit for rate limiting.
        /// </summary>
        public const int DefaultPermitLimit = 100;

        /// <summary>
        /// The default window duration in seconds.
        /// </summary>
        public const int DefaultWindowSeconds = 60;

        /// <summary>
        /// Header name for the rate limit ceiling.
        /// </summary>
        public const string HeaderLimit = "X-RateLimit-Limit";

        /// <summary>
        /// Header name for the remaining requests in the current window.
        /// </summary>
        public const string HeaderRemaining = "X-RateLimit-Remaining";

        /// <summary>
        /// Header name for the Unix timestamp when the rate limit resets.
        /// </summary>
        public const string HeaderReset = "X-RateLimit-Reset";

        /// <summary>
        /// Header name for the number of requests used in the current window.
        /// </summary>
        public const string HeaderUsed = "X-RateLimit-Used";

        /// <summary>
        /// Header name for the retry-after duration in seconds.
        /// </summary>
        public const string HeaderRetryAfter = "Retry-After";
    }

    /// <summary>
    /// Default values for HTTP Logging feature.
    /// </summary>
    public static class HttpLogging
    {
        /// <summary>
        /// Headers that are excluded from logging by default.
        /// </summary>
        public static readonly string[] ExcludedHeaders =
        [
            "Authorization",
            "Cookie",
            "Set-Cookie",
            "X-Api-Key",
            "X-Auth-Token"
        ];
    }
}
