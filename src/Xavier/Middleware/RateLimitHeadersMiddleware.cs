#if NET7_0_OR_GREATER
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Threading.RateLimiting;
using Xavier.Options;

namespace Xavier.Middleware;

/// <summary>
/// Service that tracks rate limit request counts per partition.
/// </summary>
public interface IRateLimitCounterService
{
    /// <summary>
    /// Increments the counter for the given partition and returns the current count.
    /// </summary>
    int IncrementAndGet(string partitionKey, long windowStart, int windowSeconds);

    /// <summary>
    /// Gets the current count without incrementing.
    /// </summary>
    int GetCount(string partitionKey, long windowStart);
}

/// <summary>
/// Default implementation of <see cref="IRateLimitCounterService"/>.
/// Uses a cleanup mechanism to prevent memory leaks from expired windows.
/// </summary>
public sealed class RateLimitCounterService : IRateLimitCounterService
{
    private readonly ConcurrentDictionary<string, WindowCounter> _counters = new();
    private long _lastCleanupTime;
    private const int CleanupIntervalSeconds = 300; // Clean up every 5 minutes

    /// <inheritdoc />
    public int IncrementAndGet(string partitionKey, long windowStart, int windowSeconds)
    {
        // Periodically cleanup expired entries to prevent memory leaks
        long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (currentTime - _lastCleanupTime > CleanupIntervalSeconds)
        {
            CleanupExpiredEntries(currentTime, windowSeconds);
            _lastCleanupTime = currentTime;
        }

        WindowCounter counter = _counters.AddOrUpdate(
            partitionKey,
            _ => new WindowCounter(windowStart, 1),
            (_, existing) =>
            {
                // If window has changed, reset counter
                if (existing.WindowStart != windowStart)
                {
                    return new WindowCounter(windowStart, 1);
                }
                // Otherwise increment count atomically
                return new WindowCounter(windowStart, existing.Count + 1);
            });

        return counter.Count;
    }

    /// <inheritdoc />
    public int GetCount(string partitionKey, long windowStart)
    {
        if (_counters.TryGetValue(partitionKey, out WindowCounter? counter) && counter.WindowStart == windowStart)
        {
            return counter.Count;
        }
        return 0;
    }

    private void CleanupExpiredEntries(long currentTime, int windowSeconds)
    {
        List<string> keysToRemove = _counters
            .Where(kvp => currentTime - kvp.Value.WindowStart > windowSeconds * 2) // Keep for 2 windows
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (string? key in keysToRemove)
        {
            _counters.TryRemove(key, out _);
        }
    }

    private sealed record WindowCounter(long WindowStart, int Count);
}

/// <summary>
/// Middleware that adds rate limit headers to responses for rate-limited endpoints.
/// Only adds headers and counts requests for endpoints with rate limiting policies.
/// </summary>
public sealed class RateLimitHeadersMiddleware
{
    private readonly RequestDelegate _next;
    private readonly RateLimitingOptions _options;

    /// <summary>
    /// Initializes a new instance of <see cref="RateLimitHeadersMiddleware"/>.
    /// </summary>
    public RateLimitHeadersMiddleware(
        RequestDelegate next,
        IOptions<XavierOptions> options)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _options = options?.Value?.RateLimiting ?? new RateLimitingOptions();
    }

    /// <summary>
    /// Invokes the middleware.
    /// </summary>
    public async Task InvokeAsync(HttpContext context, IRateLimitCounterService counterService)
    {
        if (!_options.Enabled || !_options.AddRateLimitHeaders)
        {
            await _next(context);
            return;
        }

        // Check if this endpoint has a rate limiting policy
        Endpoint? endpoint = context.GetEndpoint();
        EnableRateLimitingAttribute? rateLimitMetadata = endpoint?.Metadata.GetMetadata<EnableRateLimitingAttribute>();

        // Only add rate limit headers for endpoints with rate limiting enabled
        if (rateLimitMetadata == null)
        {
            await _next(context);
            return;
        }

        // Calculate window information
        int windowSeconds = _options.WindowSeconds;
        long currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        long windowStart = currentTime / windowSeconds * windowSeconds;
        long windowResetTime = windowStart + windowSeconds;

        // Get partition key for this request based on the policy
        string partitionKey = GetPartitionKey(context, rateLimitMetadata.PolicyName);

        // Get current count atomically (increments the counter)
        int used = counterService.IncrementAndGet(partitionKey, windowStart, windowSeconds);
        int remaining = Math.Max(0, _options.PermitLimit - used);

        // Capture values for the callback (these are immutable at this point)
        int limit = _options.PermitLimit;
        long reset = windowResetTime;

        // Add headers before sending response
        context.Response.OnStarting(() =>
        {
            // Only add headers if not already added (e.g., by OnRejected handler)
            if (!context.Response.Headers.ContainsKey(XavierDefaults.RateLimiting.HeaderLimit))
            {
                context.Response.Headers[XavierDefaults.RateLimiting.HeaderLimit] = limit.ToString();
                context.Response.Headers[XavierDefaults.RateLimiting.HeaderRemaining] = remaining.ToString();
                context.Response.Headers[XavierDefaults.RateLimiting.HeaderReset] = reset.ToString();
                context.Response.Headers[XavierDefaults.RateLimiting.HeaderUsed] = used.ToString();
            }

            return Task.CompletedTask;
        });

        await _next(context);
    }

    private string GetPartitionKey(HttpContext context, string? policyName)
    {
        // Use policy name as prefix to separate counters for different policies
        string prefix = policyName ?? "global";

        // For per-client policy, use client ID from header
        if (policyName == XavierDefaults.RateLimiting.PerClientPolicyName)
        {
            string clientId = context.Request.Headers[_options.ClientIdHeader].FirstOrDefault() ?? "Anonymous";
            return $"{prefix}:{clientId}";
        }

        // For per-IP policy, use IP address
        if (policyName == XavierDefaults.RateLimiting.PerIpPolicyName)
        {
            string ip = context.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
            return $"{prefix}:{ip}";
        }

        // For global policy or unknown, just use the policy name
        return prefix;
    }
}
#endif
