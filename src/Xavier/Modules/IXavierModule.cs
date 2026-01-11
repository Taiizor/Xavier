namespace Xavier.Modules;

/// <summary>
/// Interface for Xavier modules.
/// This is an internal interface used for organizing Xavier features.
/// </summary>
/// <remarks>
/// Each module represents a cross-cutting concern that can be:
/// - Enabled/disabled via configuration
/// - Configured with specific options
/// - Registered with services and middleware
/// 
/// Current modules:
/// - ErrorHandling: ProblemDetails and exception handling
/// - Correlation: Correlation ID tracking
/// - HttpLogging: HTTP request/response logging
/// - RateLimiting: Rate limiting policies
/// - OpenApi: OpenAPI document exposure
/// - Health: Health check endpoints
/// - Resilience: HttpClient resilience
/// - Observability: OpenTelemetry integration
/// </remarks>
internal interface IXavierModule
{
    /// <summary>
    /// Gets the name of the module.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets whether the module is enabled based on options.
    /// </summary>
    bool IsEnabled { get; }

    /// <summary>
    /// Configures services for the module.
    /// </summary>
    /// <param name="services">The service collection.</param>
    void ConfigureServices(Microsoft.Extensions.DependencyInjection.IServiceCollection services);

    /// <summary>
    /// Configures middleware for the module.
    /// </summary>
    /// <param name="app">The application builder.</param>
    void Configure(Microsoft.AspNetCore.Builder.IApplicationBuilder app);
}
