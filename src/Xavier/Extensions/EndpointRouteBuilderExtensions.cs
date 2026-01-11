using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Xavier.Options;

namespace Xavier.Extensions;

/// <summary>
/// Extension methods for <see cref="IEndpointRouteBuilder"/>.
/// </summary>
public static class EndpointRouteBuilderExtensions
{
    /// <summary>
    /// Maps Xavier infrastructure endpoints (health, OpenAPI).
    /// </summary>
    /// <param name="endpoints">The endpoint route builder.</param>
    /// <param name="configure">Optional configuration action.</param>
    /// <returns>The endpoint route builder for chaining.</returns>
    public static IEndpointRouteBuilder MapXavierInfrastructureEndpoints(
        this IEndpointRouteBuilder endpoints,
        Action<XavierEndpointOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(endpoints);

        XavierOptions xavierOptions = endpoints.ServiceProvider.GetService<IOptions<XavierOptions>>()?.Value ?? new XavierOptions();
        XavierEndpointOptions endpointOptions = new();
        configure?.Invoke(endpointOptions);

        string prefix = endpointOptions.RoutePrefix?.TrimEnd('/') ?? string.Empty;

        // Map health endpoints
        if (endpointOptions.MapHealthEndpoints && xavierOptions.Health.Enabled)
        {
            MapHealthEndpoints(endpoints, xavierOptions.Health, prefix);
        }

        // Map OpenAPI endpoints
        if (endpointOptions.MapOpenApiEndpoints && xavierOptions.OpenApi.Enabled)
        {
            MapOpenApiEndpoints(endpoints, xavierOptions.OpenApi, prefix);
        }

        return endpoints;
    }

    private static void MapHealthEndpoints(
        IEndpointRouteBuilder endpoints,
        HealthOptions options,
        string prefix)
    {
        string livenessPath = CombinePath(prefix, options.LivenessPath);
        string readinessPath = CombinePath(prefix, options.ReadinessPath);

        // Liveness: always healthy if app is running (no health checks)
        HealthCheckOptions livenessOptions = new()
        {
            Predicate = _ => false
        };
        if (options.IncludeDetails)
        {
            livenessOptions.ResponseWriter = WriteDetailedResponse;
        }
        endpoints.MapHealthChecks(livenessPath, livenessOptions);

        // Readiness: only checks tagged with "ready"
        HealthCheckOptions readinessOptions = new()
        {
            Predicate = check => options.ReadinessTags.Any(tag => check.Tags.Contains(tag))
        };
        if (options.IncludeDetails)
        {
            readinessOptions.ResponseWriter = WriteDetailedResponse;
        }
        endpoints.MapHealthChecks(readinessPath, readinessOptions);
    }

    private static void MapOpenApiEndpoints(
        IEndpointRouteBuilder endpoints,
        OpenApiOptions options,
        string prefix)
    {
#if NET9_0_OR_GREATER
        // Check environment for Development-only exposure
        IHostEnvironment? env = endpoints.ServiceProvider.GetService<IHostEnvironment>();
        bool isDevelopment = env?.IsDevelopment() ?? false;

        if (!isDevelopment && !options.ExposeInNonDevelopment)
        {
            return;
        }

        // Note: MapOpenApi requires Microsoft.AspNetCore.OpenApi package and AddOpenApi() service registration.
        // Xavier provides the configuration hooks, but the actual OpenAPI registration should be done by the consumer
        // via builder.Services.AddOpenApi() and app.MapOpenApi() after adding the Microsoft.AspNetCore.OpenApi package.
        // This is intentional to avoid forcing dependency on the OpenAPI package.
        //
        // Example consumer code:
        // builder.Services.AddOpenApi();
        // app.MapOpenApi();
        //
        // Xavier's XavierOptions.OpenApi can be used to configure whether OpenAPI should be exposed in production.
        _ = options; // Used for configuration
        _ = prefix; // Would be used if custom path were supported
#else
        // Built-in OpenAPI with MapOpenApi() requires .NET 9 or later.
        // On earlier TFMs, consumers should use alternative packages like Swashbuckle or NSwag.
        // Xavier still provides OpenApi options for configuration purposes.
        _ = endpoints;
        _ = options;
        _ = prefix;
#endif
    }

    private static string CombinePath(string prefix, string path)
    {
        if (string.IsNullOrEmpty(prefix))
        {
            return path;
        }

        return $"{prefix.TrimEnd('/')}/{path.TrimStart('/')}";
    }

    private static async Task WriteDetailedResponse(
        Microsoft.AspNetCore.Http.HttpContext context,
        HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            status = report.Status.ToString(),
            totalDuration = report.TotalDuration.TotalMilliseconds,
            entries = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                duration = e.Value.Duration.TotalMilliseconds,
                description = e.Value.Description,
                tags = e.Value.Tags
            })
        };

        await System.Text.Json.JsonSerializer.SerializeAsync(
            context.Response.Body,
            response,
            new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
    }
}
