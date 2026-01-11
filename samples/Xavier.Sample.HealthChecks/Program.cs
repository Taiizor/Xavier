using Microsoft.Extensions.Diagnostics.HealthChecks;

using Xavier.Extensions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add HttpClient factory for ExternalApiHealthCheck
builder.Services.AddHttpClient();

// Add Xavier with health checks
builder.AddXavier(options =>
{
    options.Health.Enabled = true;
    options.Health.LivenessPath = "/health";     // Liveness probe
    options.Health.ReadinessPath = "/ready";      // Readiness probe
    options.Health.IncludeDetails = true;         // Include detailed health info in response
    options.Health.ReadinessTags.Add("ready");    // Tag for readiness checks
});

// Add custom health checks
builder.Services.AddHealthChecks()
    // Simple always-healthy check
    .AddCheck("self", () => HealthCheckResult.Healthy("Application is running"))

    // Readiness check - tagged with "ready"
    .AddCheck<DatabaseHealthCheck>("database", tags: new[] { "ready" })

    // Readiness check - external service
    .AddCheck<ExternalApiHealthCheck>("external-api", tags: new[] { "ready" })

    // Custom degraded check example
    .AddCheck("memory", () =>
    {
        long allocated = GC.GetTotalMemory(false);
        int threshold = 100 * 1024 * 1024; // 100 MB

        if (allocated > threshold)
        {
            return HealthCheckResult.Degraded($"Memory usage is high: {allocated / 1024 / 1024} MB");
        }

        return HealthCheckResult.Healthy($"Memory usage: {allocated / 1024 / 1024} MB");
    }, tags: new[] { "ready" });

WebApplication app = builder.Build();

// Use Xavier middleware
app.UseXavier();

// Map Xavier infrastructure endpoints (includes /health and /ready)
app.MapXavierInfrastructureEndpoints();

// API endpoints
app.MapGet("/", () => "Xavier Health Checks Sample - Try /health and /ready endpoints!");

app.MapGet("/api/data", () => new { Message = "Hello from Xavier!", Timestamp = DateTime.UtcNow });

// Endpoint to simulate database state changes
bool dbHealthy = true;
app.MapGet("/api/toggle-db", () =>
{
    dbHealthy = !dbHealthy;
    return new { DatabaseHealthy = dbHealthy, Message = "Database health state toggled" };
});

// Store db health state for the health check
app.Services.GetRequiredService<IConfiguration>()["DatabaseHealthy"] = dbHealthy.ToString();

app.Run();

/// <summary>
/// Custom health check for database connectivity.
/// </summary>
public class DatabaseHealthCheck : IHealthCheck
{
    private static bool _isHealthy = true;

    public static void SetHealthy(bool isHealthy)
    {
        _isHealthy = isHealthy;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        if (_isHealthy)
        {
            return Task.FromResult(HealthCheckResult.Healthy("Database connection is healthy"));
        }

        return Task.FromResult(HealthCheckResult.Unhealthy("Cannot connect to database"));
    }
}

/// <summary>
/// Custom health check for external API connectivity.
/// </summary>
public class ExternalApiHealthCheck : IHealthCheck
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ExternalApiHealthCheck(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            HttpClient client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(5);

            // Check if we can reach an external service (using a reliable endpoint)
            HttpResponseMessage response = await client.GetAsync("https://www.google.com/robots.txt", cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                return HealthCheckResult.Healthy("External connectivity is available");
            }

            return HealthCheckResult.Degraded($"External service returned {response.StatusCode}");
        }
        catch (TaskCanceledException)
        {
            // Timeout - return degraded, not unhealthy (network might just be slow)
            return HealthCheckResult.Degraded("External connectivity check timed out");
        }
        catch (HttpRequestException)
        {
            // Network error - in some environments (like CI), external access is blocked
            // Return degraded instead of unhealthy to allow the sample to run
            return HealthCheckResult.Degraded("External connectivity unavailable (network restricted)");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("External connectivity check failed", ex);
        }
    }
}
