# Xavier

[![NuGet](https://img.shields.io/nuget/v/Xavier.svg)](https://www.nuget.org/packages/Xavier)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

**Opinionated but customizable API defaults pack for ASP.NET Core.**

Xavier bundles common cross-cutting concerns into a simple, one-liner setup — no boilerplate required.

## Install

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

## What You Get

| Feature              | Description                                         | Default  |
| -------------------- | --------------------------------------------------- | -------- |
| **ProblemDetails**   | RFC 7807 error responses with trace/correlation IDs | Enabled  |
| **Correlation ID**   | Request tracking with `X-Correlation-Id` header     | Enabled  |
| **Health Endpoints** | `/health` (liveness) and `/ready` (readiness)       | Enabled  |
| **OpenAPI**          | Document exposure at `/openapi/v1.json` (net8+)     | Enabled  |
| **HTTP Logging**     | Safe-by-default request/response logging            | Disabled |
| **Rate Limiting**    | Global, Per-IP, Per-Client policies (net7+)         | Disabled |
| **Resilience**       | HttpClient retry/timeout/circuit-breaker (net8+)    | Disabled |
| **Observability**    | OpenTelemetry wiring (no forced exporters)          | Disabled |

## Framework Support

| Feature          | net6.0 | net7.0 | net8.0 | net9.0 | net10.0 |
| ---------------- | :----: | :----: | :----: | :----: | :-----: |
| Error Handling   |  Yes   |  Yes   |  Yes   |  Yes   |   Yes   |
| Correlation ID   |  Yes   |  Yes   |  Yes   |  Yes   |   Yes   |
| Health Endpoints |  Yes   |  Yes   |  Yes   |  Yes   |   Yes   |
| HTTP Logging     |  Yes   |  Yes   |  Yes   |  Yes   |   Yes   |
| Rate Limiting    |   -    |  Yes   |  Yes   |  Yes   |   Yes   |
| OpenAPI          |   -    |   -    |  Yes   |  Yes   |   Yes   |
| Resilience       |   -    |   -    |  Yes   |  Yes   |   Yes   |
| Observability    |  Yes   |  Yes   |  Yes   |  Yes   |   Yes   |

Unsupported features gracefully no-op (warning logged, no crash).

## Configuration

All features are configurable via code or `appsettings.json`:

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

Or via code:

```csharp
builder.AddXavier(options =>
{
    options.RateLimiting.Enabled = true;
    options.RateLimiting.PermitLimit = 100;
    options.Observability.Enabled = true;
    options.Observability.ServiceName = "MyApi";
});
```

## Security Defaults

-   **Exception details** hidden in production
-   **Sensitive headers** (Authorization, Cookie) excluded from HTTP logging
-   **OpenAPI** only exposed in Development environment
-   **Rate limiting** available to prevent abuse

## Links

-   [Full Documentation](https://github.com/Taiizor/Xavier#readme)
-   [Samples](https://github.com/Taiizor/Xavier/tree/main/samples)
-   [Changelog](https://github.com/Taiizor/Xavier/blob/main/CHANGELOG.md)
-   [Issues](https://github.com/Taiizor/Xavier/issues)

## License

MIT - [Taiizor](https://github.com/Taiizor)
