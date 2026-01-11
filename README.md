# Xavier

[![NuGet](https://img.shields.io/nuget/v/Xavier.svg)](https://www.nuget.org/packages/Xavier)
[![Build Status](https://github.com/Taiizor/Xavier/actions/workflows/ci.yml/badge.svg)](https://github.com/Taiizor/Xavier/actions/workflows/ci.yml)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

**Xavier** is an opinionated but customizable "API Defaults Pack" for ASP.NET Core. It bundles common cross-cutting concerns into a simple setup—no boilerplate required.

## Philosophy

Xavier focuses on:

-   **Safe defaults** — Secure configurations out of the box
-   **One-liner adoption** — Minimal code to get production-ready features
-   **Customizable** — Every feature can be configured or disabled
-   **Non-invasive** — Doesn't replace ASP.NET Core patterns

## Installation

```bash
dotnet add package Xavier
```

## Quick Start

### Minimal API

```csharp
using Xavier.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.AddXavier();

var app = builder.Build();
app.UseXavier();
app.MapXavierInfrastructureEndpoints();

app.MapGet("/", () => "Hello World!");
app.Run();
```

### Controllers

```csharp
using Xavier.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.AddXavier();

var app = builder.Build();
app.UseXavier();
app.MapControllers();
app.MapXavierInfrastructureEndpoints();

app.Run();
```

## Features

### Error Handling & ProblemDetails (Default: Enabled)

Converts exceptions and non-success status codes to RFC 7807 ProblemDetails:

```json
{
    "type": "https://tools.ietf.org/html/rfc9110#section-15.6.1",
    "title": "Internal Server Error",
    "status": 500,
    "traceId": "00-abc123...",
    "correlationId": "f47ac10b58cc4372"
}
```

**Configuration:**

```csharp
builder.AddXavier(options =>
{
    options.ErrorHandling.IncludeExceptionDetails = true; // Only in Development!
    options.ErrorHandling.IncludeTraceId = true;
    options.ErrorHandling.IncludeCorrelationId = true;
});
```

### Correlation ID (Default: Enabled)

Tracks requests with correlation IDs:

-   Accepts `X-Correlation-Id` header
-   Optionally accepts W3C `traceparent` headers (opt-in via `UseTraceparent`)
-   Generates ID if none provided
-   Adds `X-Correlation-Id` to response headers
-   Enriches log scopes with `correlation_id`, `trace_id`, `span_id`

**Configuration:**

```csharp
options.Correlation.HeaderName = "X-Request-Id"; // Custom header
options.Correlation.UseTraceparent = true; // Enable traceparent fallback (default: false)
options.Correlation.EnrichLogScopes = true;
```

### Health Endpoints (Default: Enabled)

Maps health check endpoints:

-   `/health` — Liveness (always healthy if app is running)
-   `/ready` — Readiness (checks tagged with "ready")

**Configuration:**

```csharp
options.Health.LivenessPath = "/healthz";
options.Health.ReadinessPath = "/readyz";
options.Health.IncludeDetails = true; // JSON response with check details
```

### HTTP Logging (Default: Disabled)

Safe-by-default HTTP request/response logging:

```csharp
options.HttpLogging.Enabled = true;
options.HttpLogging.LogRequestBody = false;  // Keep false for security
options.HttpLogging.LogResponseBody = false;
options.HttpLogging.ExcludedHeaders = ["Authorization", "Cookie"];
```

### Rate Limiting (Default: Disabled, .NET 7+)

Built-in rate limiting policies with response headers:

```csharp
options.RateLimiting.Enabled = true;
options.RateLimiting.EnableGlobalPolicy = true;
options.RateLimiting.EnablePerIpPolicy = true;
options.RateLimiting.PermitLimit = 100;
options.RateLimiting.WindowSeconds = 60;
options.RateLimiting.AddRateLimitHeaders = true; // Default: true
```

**Rate Limit Headers (like GitHub API):**
- `X-RateLimit-Limit`: Maximum requests allowed
- `X-RateLimit-Remaining`: Requests remaining in window (decreases with each request)
- `X-RateLimit-Used`: Requests used in window (increases with each request)
- `X-RateLimit-Reset`: Unix timestamp when limit resets
- `Retry-After`: Seconds to wait (on 429 responses)

### OpenAPI (Default: Enabled, .NET 8+)

Exposes OpenAPI document (Development only by default):

```csharp
options.OpenApi.DocumentName = "v1";
options.OpenApi.PathTemplate = "/openapi/{documentName}.json";
options.OpenApi.ExposeInNonDevelopment = false; // Set true to expose outside Development
```

> **Note:** Xavier does NOT ship SwaggerUI. Add it yourself if needed.

### HTTP Resilience (Default: Disabled, .NET 8+)

Standard resilience for HttpClient:

```csharp
// Option 1: Apply to all clients
builder.Services.AddXavierHttpClientDefaults();

// Option 2: Apply to specific client
builder.Services.AddHttpClient("MyApi")
    .AddXavierDefaults(); // Adds retry, timeout, circuit breaker
```

### Observability (Default: Disabled)

OpenTelemetry wiring without forced exporters:

```csharp
options.Observability.Enabled = true;
options.Observability.ServiceName = "MyApi";
options.Observability.EnableTracing = true;
options.Observability.EnableMetrics = true;

// Add exporters via callbacks (Xavier does NOT auto-enable exporters)
options.Observability.ConfigureTracing = tracing =>
    tracing.AddOtlpExporter(o => o.Endpoint = new Uri("http://localhost:4317"));
options.Observability.ConfigureMetrics = metrics =>
    metrics.AddOtlpExporter(o => o.Endpoint = new Uri("http://localhost:4317"));
```

## Configuration via appsettings.json

```json
{
    "Xavier": {
        "Correlation": {
            "Enabled": true,
            "HeaderName": "X-Correlation-Id"
        },
        "ErrorHandling": {
            "Enabled": true,
            "IncludeExceptionDetails": false
        },
        "HttpLogging": {
            "Enabled": false
        },
        "RateLimiting": {
            "Enabled": false,
            "PermitLimit": 100,
            "WindowSeconds": 60
        },
        "OpenApi": {
            "Enabled": true,
            "DocumentName": "v1"
        },
        "Health": {
            "Enabled": true,
            "LivenessPath": "/health",
            "ReadinessPath": "/ready"
        },
        "Observability": {
            "Enabled": false
        }
    }
}
```

## Feature Matrix

| Feature            | net6.0 | net7.0 | net8.0 | net9.0 | net10.0 |
| ------------------ | :----: | :----: | :----: | :----: | :-----: |
| Error Handling     |   ✅   |   ✅   |   ✅   |   ✅   |   ✅    |
| Correlation ID     |   ✅   |   ✅   |   ✅   |   ✅   |   ✅    |
| HTTP Logging       |   ✅   |   ✅   |   ✅   |   ✅   |   ✅    |
| Rate Limiting      |   ❌   |   ✅   |   ✅   |   ✅   |   ✅    |
| OpenAPI (built-in) |   ❌   |   ❌   |   ✅   |   ✅   |   ✅    |
| Health Endpoints   |   ✅   |   ✅   |   ✅   |   ✅   |   ✅    |
| HTTP Resilience    |   ❌   |   ❌   |   ✅   |   ✅   |   ✅    |
| Observability      |   ✅   |   ✅   |   ✅   |   ✅   |   ✅    |

❌ = Graceful no-op (warning logged, no crash)

## Security Considerations

-   **HTTP Logging**: Sensitive headers (Authorization, Cookie) are excluded by default
-   **Exception Details**: Hidden in production by default
-   **OpenAPI**: Only exposed in Development environment by default
-   **Rate Limiting**: Enable to prevent abuse

## Samples

See the [samples](samples/) directory:

### Quick Start Samples

-   [Minimal API Sample](samples/Xavier.Sample.MinimalApi/) - Basic Xavier usage with minimal API
-   [Controllers API Sample](samples/Xavier.Sample.ControllersApi/) - Xavier with MVC controllers

### Feature-Specific Samples

-   [Rate Limiting Sample](samples/Xavier.Sample.RateLimiting/) - Demonstrates global, per-IP, and per-client rate limiting
-   [Observability Sample](samples/Xavier.Sample.Observability/) - OpenTelemetry tracing and metrics with exporter configuration
-   [HTTP Logging Sample](samples/Xavier.Sample.HttpLogging/) - Safe HTTP request/response logging with sensitive header exclusion
-   [Resilience Sample](samples/Xavier.Sample.Resilience/) - HttpClient retry, timeout, and circuit breaker patterns
-   [Health Checks Sample](samples/Xavier.Sample.HealthChecks/) - Kubernetes-ready liveness and readiness endpoints

## Contributing

Contributions are welcome! Please read our [Contributing Guidelines](CONTRIBUTING.md).

## License

MIT © [Taiizor](https://github.com/Taiizor)

See [LICENSE](LICENSE) for details.
