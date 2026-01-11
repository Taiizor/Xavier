using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using System.Diagnostics;
using Xavier.Options;

namespace Xavier.Middleware;

/// <summary>
/// Middleware for handling correlation IDs in HTTP requests.
/// </summary>
public sealed class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;
    private readonly CorrelationOptions _options;

    /// <summary>
    /// Initializes a new instance of <see cref="CorrelationIdMiddleware"/>.
    /// </summary>
    public CorrelationIdMiddleware(
        RequestDelegate next,
        ILogger<CorrelationIdMiddleware> logger,
        IOptions<XavierOptions> options)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value?.Correlation ?? new CorrelationOptions();
    }

    /// <summary>
    /// Invokes the middleware.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        string correlationId = GetOrCreateCorrelationId(context);

        context.Items["CorrelationId"] = correlationId;

        if (_options.AddToResponse)
        {
            context.Response.OnStarting(() =>
            {
                if (!context.Response.Headers.ContainsKey(_options.HeaderName))
                {
                    context.Response.Headers[_options.HeaderName] = correlationId;
                }
                return Task.CompletedTask;
            });
        }

        if (_options.EnrichLogScopes)
        {
            Dictionary<string, object?> scopeData = new()
            {
                ["correlation_id"] = correlationId,
                ["trace_id"] = Activity.Current?.TraceId.ToString(),
                ["span_id"] = Activity.Current?.SpanId.ToString()
            };

            using (_logger.BeginScope(scopeData))
            {
                await _next(context);
            }
        }
        else
        {
            await _next(context);
        }
    }

    private string GetOrCreateCorrelationId(HttpContext context)
    {
        // Try to get from custom header
        if (context.Request.Headers.TryGetValue(_options.HeaderName, out StringValues headerValue) &&
            !string.IsNullOrWhiteSpace(headerValue))
        {
            return headerValue.ToString();
        }

        // Try to get from W3C traceparent header
        if (_options.UseTraceparent &&
            context.Request.Headers.TryGetValue(XavierDefaults.Correlation.TraceparentHeader, out StringValues traceparent) &&
            !string.IsNullOrWhiteSpace(traceparent))
        {
            string[] parts = traceparent.ToString().Split('-');
            if (parts.Length >= 2 && !string.IsNullOrWhiteSpace(parts[1]))
            {
                return parts[1];
            }
        }

        // Generate new correlation ID
        return Guid.NewGuid().ToString("N");
    }
}
