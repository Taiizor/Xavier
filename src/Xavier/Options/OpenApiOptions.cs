namespace Xavier.Options;

/// <summary>
/// Options for OpenAPI feature.
/// </summary>
public sealed class OpenApiOptions
{
    /// <summary>
    /// Whether OpenAPI is enabled. Default: true (on net8.0+).
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// The OpenAPI document name. Default: "v1".
    /// </summary>
    public string DocumentName { get; set; } = XavierDefaults.OpenApi.DocumentName;

    /// <summary>
    /// The path template for OpenAPI document. Default: "/openapi/{documentName}.json".
    /// </summary>
    public string PathTemplate { get; set; } = XavierDefaults.OpenApi.PathTemplate;

    /// <summary>
    /// Whether to expose OpenAPI in non-Development environments. Default: false.
    /// By default, OpenAPI is only exposed in Development environment.
    /// </summary>
    public bool ExposeInNonDevelopment { get; set; }

    /// <summary>
    /// The title for the OpenAPI document.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// The version for the OpenAPI document.
    /// </summary>
    public string? Version { get; set; }
}
