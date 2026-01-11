using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Xavier.Diagnostics;
using Xavier.Internal;
using Xavier.Middleware;
using Xavier.Options;
using HttpLoggingOptions = Xavier.Options.HttpLoggingOptions;

#if NET7_0_OR_GREATER
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
#endif

namespace Xavier.Extensions;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Xavier services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <param name="configure">Optional configuration action.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddXavier(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<XavierOptions>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        // Bind options from configuration
        IConfigurationSection optionsSection = configuration.GetSection(XavierDefaults.ConfigurationSection);
        services.Configure<XavierOptions>(optionsSection);

        // Allow programmatic configuration
        if (configure is not null)
        {
            services.PostConfigure(configure);
        }

        // Build options for immediate use
        XavierOptions options = new();
        optionsSection.Bind(options);
        configure?.Invoke(options);

        // Add services based on options
        ConfigureErrorHandling(services, options.ErrorHandling);
        ConfigureHttpLogging(services, options.HttpLogging);
        ConfigureRateLimiting(services, options.RateLimiting);
        ConfigureHealth(services, options.Health);
        ConfigureOpenApi(services, options.OpenApi);
        ConfigureObservability(services, options.Observability);

        return services;
    }

    private static void ConfigureErrorHandling(IServiceCollection services, ErrorHandlingOptions options)
    {
        if (!options.Enabled)
        {
            return;
        }

#if NET7_0_OR_GREATER
        services.AddProblemDetails(problemDetails =>
        {
            problemDetails.CustomizeProblemDetails = ctx =>
            {
                ProblemDetailsHelpers.EnrichProblemDetails(
                    ctx.HttpContext,
                    ctx.ProblemDetails.Extensions,
                    options.IncludeTraceId,
                    options.IncludeCorrelationId);
            };
        });
#else
        // ProblemDetails service registration is not available in net6.0
        // The middleware will still work with ASP.NET Core's built-in error handling
#endif
    }

    private static void ConfigureHttpLogging(IServiceCollection services, HttpLoggingOptions options)
    {
        if (!options.Enabled)
        {
            return;
        }

        services.AddHttpLogging(logging =>
        {
            // Configure logging fields
            HttpLoggingFields fields = HttpLoggingFields.RequestPath |
                         HttpLoggingFields.RequestMethod |
                         HttpLoggingFields.ResponseStatusCode;

#if NET8_0_OR_GREATER
            fields |= HttpLoggingFields.Duration;
#endif

            if (options.LogRequestHeaders)
            {
                fields |= HttpLoggingFields.RequestHeaders;
            }

            if (options.LogResponseHeaders)
            {
                fields |= HttpLoggingFields.ResponseHeaders;
            }

            if (options.LogRequestBody)
            {
                fields |= HttpLoggingFields.RequestBody;
            }

            if (options.LogResponseBody)
            {
                fields |= HttpLoggingFields.ResponseBody;
            }

            logging.LoggingFields = fields;
            logging.RequestBodyLogLimit = options.RequestBodyLogLimit;
            logging.ResponseBodyLogLimit = options.ResponseBodyLogLimit;

            // Remove sensitive headers
            foreach (string header in options.ExcludedHeaders)
            {
                logging.RequestHeaders.Remove(header);
                logging.ResponseHeaders.Remove(header);
            }
        });
    }

    private static void ConfigureRateLimiting(IServiceCollection services, RateLimitingOptions options)
    {
        if (!options.Enabled)
        {
            return;
        }

#if NET7_0_OR_GREATER
        // Register the rate limit counter service as singleton per host
        services.AddSingleton<IRateLimitCounterService, RateLimitCounterService>();

        services.AddRateLimiter(limiter =>
        {
            if (options.EnableGlobalPolicy)
            {
                limiter.AddFixedWindowLimiter(XavierDefaults.RateLimiting.GlobalPolicyName, opt =>
                {
                    opt.PermitLimit = options.PermitLimit;
                    opt.Window = TimeSpan.FromSeconds(options.WindowSeconds);
                    opt.QueueLimit = options.QueueLimit;
                    opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                });
            }

            if (options.EnablePerIpPolicy)
            {
                limiter.AddPolicy(XavierDefaults.RateLimiting.PerIpPolicyName, context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        context.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                        _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = options.PermitLimit,
                            Window = TimeSpan.FromSeconds(options.WindowSeconds),
                            QueueLimit = options.QueueLimit,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                        }));
            }

            if (options.EnablePerClientPolicy)
            {
                limiter.AddPolicy(XavierDefaults.RateLimiting.PerClientPolicyName, context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        context.Request.Headers[options.ClientIdHeader].FirstOrDefault() ?? "Anonymous",
                        _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = options.PermitLimit,
                            Window = TimeSpan.FromSeconds(options.WindowSeconds),
                            QueueLimit = options.QueueLimit,
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                        }));
            }

            limiter.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // Configure rejection handler to add rate limit headers and return proper response
            limiter.OnRejected = async (context, cancellationToken) =>
            {
                HttpResponse response = context.HttpContext.Response;

                // Set content type
                response.ContentType = "application/problem+json";

                // Add rate limit headers
                if (options.AddRateLimitHeaders)
                {
                    // Calculate window reset time using fixed window calculation
                    int windowSeconds = options.WindowSeconds;
                    long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    long windowStart = currentTime / windowSeconds * windowSeconds;
                    long windowResetTime = windowStart + windowSeconds;

                    int retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retryAfterValue)
                        ? (int)retryAfterValue.TotalSeconds
                        : (int)(windowResetTime - currentTime);

                    response.Headers[XavierDefaults.RateLimiting.HeaderLimit] = options.PermitLimit.ToString();
                    response.Headers[XavierDefaults.RateLimiting.HeaderRemaining] = "0";
                    response.Headers[XavierDefaults.RateLimiting.HeaderReset] = windowResetTime.ToString();
                    response.Headers[XavierDefaults.RateLimiting.HeaderUsed] = options.PermitLimit.ToString();
                    response.Headers[XavierDefaults.RateLimiting.HeaderRetryAfter] = retryAfter.ToString();
                }

                // Write a JSON response body
                var problemDetails = new
                {
                    type = "https://tools.ietf.org/html/rfc6585#section-4",
                    title = "Too Many Requests",
                    status = StatusCodes.Status429TooManyRequests,
                    detail = "Rate limit exceeded. Please try again later."
                };

                await response.WriteAsJsonAsync(problemDetails, cancellationToken);
            };
        });
#else
        // Log warning about unsupported feature (once per process)
        ServiceProvider sp = services.BuildServiceProvider();
        ILogger<XavierOptions>? logger = sp.GetService<ILogger<XavierOptions>>();
        logger?.LogUnsupportedFeatureOnce("Rate limiting", "net7.0", FeatureDetection.CurrentTfm);
#endif
    }

    private static void ConfigureHealth(IServiceCollection services, HealthOptions options)
    {
        if (!options.Enabled)
        {
            return;
        }

        services.AddHealthChecks();
    }

    private static void ConfigureOpenApi(IServiceCollection services, OpenApiOptions options)
    {
        if (!options.Enabled)
        {
            return;
        }

#if NET8_0_OR_GREATER
        // Built-in OpenAPI is available - configuration handled in MapXavierInfrastructureEndpoints
#else
        // Log warning about unsupported feature (once per process)
        ServiceProvider sp = services.BuildServiceProvider();
        ILogger<XavierOptions>? logger = sp.GetService<ILogger<XavierOptions>>();
        logger?.LogUnsupportedFeatureOnce("Built-in OpenAPI", "net8.0", FeatureDetection.CurrentTfm);
#endif
    }

    private static void ConfigureObservability(IServiceCollection services, ObservabilityOptions options)
    {
        if (!options.Enabled)
        {
            return;
        }

        string serviceName = options.ServiceName ?? "Unknown";
        string serviceVersion = options.ServiceVersion ?? "1.0.0";

        OpenTelemetryBuilder otelBuilder = services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(serviceName, serviceVersion: serviceVersion));

        if (options.EnableTracing)
        {
            otelBuilder.WithTracing(tracing =>
            {
                tracing.AddAspNetCoreInstrumentation();
                tracing.AddHttpClientInstrumentation();

                // Note: Exporters are NOT auto-enabled by Xavier.
                // To add exporters, use the ConfigureTracing callback in AddXavier:
                // builder.AddXavier(opts => {
                //     opts.Observability.Enabled = true;
                //     opts.Observability.ConfigureTracing = tracing => tracing.AddOtlpExporter();
                // });
                options.ConfigureTracing?.Invoke(tracing);
            });
        }

        if (options.EnableMetrics)
        {
            otelBuilder.WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation();
                metrics.AddHttpClientInstrumentation();

                if (options.EnableRuntimeMetrics)
                {
                    metrics.AddRuntimeInstrumentation();
                }

                // Note: Exporters are NOT auto-enabled by Xavier.
                // To add exporters, use the ConfigureMetrics callback in AddXavier:
                // builder.AddXavier(opts => {
                //     opts.Observability.Enabled = true;
                //     opts.Observability.ConfigureMetrics = metrics => metrics.AddOtlpExporter();
                // });
                options.ConfigureMetrics?.Invoke(metrics);
            });
        }
    }

    /// <summary>
    /// Adds Xavier default HttpClient configuration with standard resilience for all clients.
    /// This is a convenience method that configures default HttpClient options.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    /// <remarks>
    /// Note: Standard resilience requires .NET 8 or later. On earlier TFMs, this is a no-op.
    /// For more control, use AddHttpClient().AddXavierDefaults() on individual clients.
    /// </remarks>
    public static IServiceCollection AddXavierHttpClientDefaults(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

#if NET8_0_OR_GREATER
        services.ConfigureHttpClientDefaults(builder =>
        {
            builder.AddStandardResilienceHandler();
        });
#else
        // Log warning about unsupported feature (once per process)
        ServiceProvider sp = services.BuildServiceProvider();
        ILogger<XavierOptions>? logger = sp.GetService<ILogger<XavierOptions>>();
        logger?.LogUnsupportedFeatureOnce("HttpClient resilience", "net8.0", FeatureDetection.CurrentTfm);
#endif

        return services;
    }
}
