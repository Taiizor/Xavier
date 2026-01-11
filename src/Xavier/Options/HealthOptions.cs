namespace Xavier.Options;

/// <summary>
/// Options for health endpoints feature.
/// </summary>
public sealed class HealthOptions
{
    /// <summary>
    /// Whether health endpoints are enabled. Default: true.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// The path for liveness health check. Default: "/health".
    /// </summary>
    public string LivenessPath { get; set; } = XavierDefaults.Health.LivenessPath;

    /// <summary>
    /// The path for readiness health check. Default: "/ready".
    /// </summary>
    public string ReadinessPath { get; set; } = XavierDefaults.Health.ReadinessPath;

    /// <summary>
    /// Tags used to filter readiness health checks. Default: ["ready"].
    /// </summary>
    public List<string> ReadinessTags { get; set; } = [XavierDefaults.Health.ReadinessTag];

    /// <summary>
    /// Whether to include detailed health check results in response. Default: false.
    /// </summary>
    public bool IncludeDetails { get; set; }
}
