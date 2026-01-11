namespace Xavier.Options;

/// <summary>
/// Options for MapXavierInfrastructureEndpoints() configuration.
/// </summary>
public sealed class XavierEndpointOptions
{
    /// <summary>
    /// Whether to map health endpoints. Default: true.
    /// </summary>
    public bool MapHealthEndpoints { get; set; } = true;

    /// <summary>
    /// Whether to map OpenAPI endpoints. Default: true (on net8.0+).
    /// </summary>
    public bool MapOpenApiEndpoints { get; set; } = true;

    /// <summary>
    /// Route prefix for all infrastructure endpoints. Default: empty.
    /// </summary>
    public string? RoutePrefix { get; set; }

    /// <summary>
    /// Tags to apply to infrastructure endpoints.
    /// </summary>
    public List<string> EndpointTags { get; set; } = [];
}
