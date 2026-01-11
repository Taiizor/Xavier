using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Text.Json;
using Xavier.Middleware;
using Xavier.Options;

namespace Xavier.Extensions;

/// <summary>
/// Extension methods for <see cref="IApplicationBuilder"/>.
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Adds Xavier middleware to the application pipeline.
    /// Call this after UseRouting() to ensure endpoint metadata is available for rate limiting.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <param name="configure">Optional configuration action.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseXavier(
        this IApplicationBuilder app,
        Action<XavierAppOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(app);

        XavierOptions xavierOptions = app.ApplicationServices.GetService<IOptions<XavierOptions>>()?.Value ?? new XavierOptions();
        XavierAppOptions appOptions = new();
        configure?.Invoke(appOptions);

        // 1. Correlation ID (early in pipeline)
        if (appOptions.UseCorrelation && xavierOptions.Correlation.Enabled)
        {
            app.UseMiddleware<CorrelationIdMiddleware>();
        }

        // 2. Error handling with custom exception handler that respects IncludeExceptionDetails
        if (appOptions.UseErrorHandling && xavierOptions.ErrorHandling.Enabled)
        {
            ErrorHandlingOptions errorOptions = xavierOptions.ErrorHandling;
            IHostEnvironment? env = app.ApplicationServices.GetService<IHostEnvironment>();
            bool isDevelopment = env?.IsDevelopment() ?? false;

            // Include exception details only if explicitly enabled OR in development
            bool includeDetails = errorOptions.IncludeExceptionDetails || isDevelopment;

            app.UseExceptionHandler(exceptionApp =>
            {
                exceptionApp.Run(async context =>
                {
                    IExceptionHandlerFeature? exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
                    Exception? exception = exceptionFeature?.Error;

                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    context.Response.ContentType = "application/problem+json";

                    // Build ProblemDetails-like response
                    Dictionary<string, object?> problemDetails = new()
                    {
                        ["type"] = "https://tools.ietf.org/html/rfc9110#section-15.6.1",
                        ["title"] = "An error occurred while processing your request.",
                        ["status"] = StatusCodes.Status500InternalServerError
                    };

                    // Include exception details only if enabled (for development/debugging)
                    if (includeDetails && exception is not null)
                    {
                        problemDetails["detail"] = exception.Message;
                        problemDetails["exception"] = new
                        {
                            type = exception.GetType().FullName,
                            message = exception.Message,
                            stackTrace = exception.StackTrace
                        };
                    }

                    // Include trace ID if enabled
                    if (errorOptions.IncludeTraceId)
                    {
                        string traceId = Activity.Current?.Id ?? context.TraceIdentifier;
                        problemDetails["traceId"] = traceId;
                    }

                    // Include correlation ID if enabled
                    if (errorOptions.IncludeCorrelationId && context.Items.TryGetValue("CorrelationId", out object? correlationId))
                    {
                        problemDetails["correlationId"] = correlationId?.ToString();
                    }

                    await context.Response.WriteAsync(
                        JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions { WriteIndented = false }));
                });
            });
            app.UseStatusCodePages();
        }

        // 3. Rate limiting with headers (net7+)
        // Note: RateLimitHeadersMiddleware requires routing to have run first
        // to access endpoint metadata (EnableRateLimitingAttribute).
        // It only adds headers to endpoints with rate limiting enabled.
        if (appOptions.UseRateLimiting && xavierOptions.RateLimiting.Enabled)
        {
#if NET7_0_OR_GREATER
            // Add rate limit headers middleware before rate limiter
            // This middleware only counts/adds headers for endpoints with rate limiting policies
            if (xavierOptions.RateLimiting.AddRateLimitHeaders)
            {
                app.UseMiddleware<RateLimitHeadersMiddleware>();
            }
            app.UseRateLimiter();
#endif
        }

        // 4. HTTP logging
        if (appOptions.UseHttpLogging && xavierOptions.HttpLogging.Enabled)
        {
            app.UseHttpLogging();
        }

        return app;
    }
}
