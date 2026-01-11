using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

using Xavier.Options;

#if NET8_0_OR_GREATER
using Microsoft.Extensions.Http.Resilience;
#endif

namespace Xavier.Extensions;

/// <summary>
/// Extension methods for <see cref="WebApplicationBuilder"/> and <see cref="WebApplication"/>.
/// </summary>
public static class WebApplicationExtensions
{
    /// <summary>
    /// Adds Xavier services to the web application builder.
    /// </summary>
    /// <param name="builder">The web application builder.</param>
    /// <param name="configure">Optional configuration action.</param>
    /// <returns>The web application builder for chaining.</returns>
    public static WebApplicationBuilder AddXavier(
        this WebApplicationBuilder builder,
        Action<XavierOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddXavier(builder.Configuration, configure);
        return builder;
    }

    /// <summary>
    /// Adds Xavier middleware and maps infrastructure endpoints.
    /// </summary>
    /// <param name="app">The web application.</param>
    /// <param name="configure">Optional configuration action.</param>
    /// <returns>The web application for chaining.</returns>
    public static WebApplication UseXavier(
        this WebApplication app,
        Action<XavierAppOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(app);

        ((IApplicationBuilder)app).UseXavier(configure);
        return app;
    }

    /// <summary>
    /// Maps Xavier infrastructure endpoints.
    /// </summary>
    /// <param name="app">The web application.</param>
    /// <param name="configure">Optional configuration action.</param>
    /// <returns>The web application for chaining.</returns>
    public static WebApplication MapXavierInfrastructureEndpoints(
        this WebApplication app,
        Action<XavierEndpointOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(app);

        ((IEndpointRouteBuilder)app).MapXavierInfrastructureEndpoints(configure);
        return app;
    }
}

/// <summary>
/// Extension methods for HttpClient resilience.
/// </summary>
public static class HttpClientBuilderExtensions
{
    /// <summary>
    /// Adds Xavier default resilience to an HttpClient.
    /// </summary>
    /// <param name="builder">The HttpClient builder.</param>
    /// <returns>The HttpClient builder for chaining.</returns>
    public static IHttpClientBuilder AddXavierDefaults(this IHttpClientBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

#if NET8_0_OR_GREATER
        builder.AddStandardResilienceHandler();
#endif

        return builder;
    }
}
