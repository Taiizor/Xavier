# Xavier Modules Reference

Xavier organizes its features into modular components. Each module can be individually enabled or disabled.

## Module Overview

| Module         | Default State | Description                                       |
| -------------- | ------------- | ------------------------------------------------- |
| ErrorHandling  | Enabled       | Consistent error responses with ProblemDetails    |
| Correlation    | Enabled       | Request tracking with correlation IDs             |
| HttpLogging    | Disabled      | HTTP request/response logging                     |
| RateLimiting   | Disabled      | Request rate limiting                             |
| OpenApi        | Enabled       | OpenAPI document exposure                         |
| Health         | Enabled       | Health check endpoints                            |
| Resilience     | Disabled      | HttpClient resilience patterns                    |
| Observability  | Disabled      | OpenTelemetry integration                         |

## Error Handling Module

**Purpose:** Convert exceptions and non-success status codes to RFC 7807 ProblemDetails responses.

**Services Registered:**
- `AddProblemDetails()` (net7.0+)

**Middleware Used:**
- `UseExceptionHandler()`
- `UseStatusCodePages()`

**Response Example:**
```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.6.1",
  "title": "Internal Server Error",
  "status": 500,
  "traceId": "00-abc123...",
  "correlationId": "f47ac10b58cc4372"
}
```

**Security Notes:**
- Exception details are hidden by default in non-Development environments
- Only enable `IncludeExceptionDetails` in Development

## Correlation Module

**Purpose:** Track requests across services with correlation IDs.

**Middleware:** `CorrelationIdMiddleware`

**Behavior:**
1. Check for `X-Correlation-Id` header in request
2. If missing and `UseTraceparent` is true, try to extract from `traceparent` header
3. If still missing, generate a new GUID
4. Store correlation ID in `HttpContext.Items["CorrelationId"]`
5. Add `X-Correlation-Id` to response headers
6. Enrich log scope with `correlation_id`, `trace_id`, `span_id`

**Log Scope Properties:**
- `correlation_id`: The correlation ID (custom header or generated)
- `trace_id`: The W3C trace ID from Activity.Current
- `span_id`: The W3C span ID from Activity.Current

## HTTP Logging Module

**Purpose:** Safe-by-default HTTP request/response logging.

**Services Registered:**
- `AddHttpLogging()`

**Middleware Used:**
- `UseHttpLogging()`

**Default Excluded Headers:**
- Authorization
- Cookie
- Set-Cookie
- X-Api-Key
- X-Auth-Token

**Security Notes:**
- Request/response bodies are NOT logged by default
- Sensitive headers are excluded by default
- Configure `ExcludedHeaders` for additional exclusions

## Rate Limiting Module

**Purpose:** Protect APIs from abuse with built-in rate limiting.

**Requires:** .NET 7.0 or later

**Services Registered:**
- `AddRateLimiter()`

**Middleware Used:**
- `UseRateLimiter()`

**Built-in Policies:**
| Policy Name        | Key            | Description              |
| ------------------ | -------------- | ------------------------ |
| Xavier.Global      | (none)         | Global rate limit        |
| Xavier.PerIP       | Remote IP      | Per-IP rate limit        |
| Xavier.PerClient   | X-Client-Id    | Per-client rate limit    |

**Algorithm:** Fixed Window

**Usage with Endpoints:**
```csharp
app.MapGet("/api/data", () => "Hello")
   .RequireRateLimiting("Xavier.PerIP");
```

## OpenAPI Module

**Purpose:** Expose OpenAPI (Swagger) document.

**Requires:** .NET 9.0+ for `MapOpenApi()`

**Default Behavior:**
- Enabled only in Development environment
- Document path: `/openapi/v1.json`
- No UI included (add SwaggerUI separately if needed)

**Notes:**
- Xavier provides configuration hooks but consumers must add `Microsoft.AspNetCore.OpenApi` package
- Use `AddOpenApi()` and `MapOpenApi()` in consumer code

## Health Module

**Purpose:** Expose health check endpoints for orchestrators.

**Services Registered:**
- `AddHealthChecks()`

**Endpoints Mapped:**
| Path     | Purpose    | Health Checks             |
| -------- | ---------- | ------------------------- |
| /health  | Liveness   | None (always healthy)     |
| /ready   | Readiness  | Tagged with "ready"       |

**Liveness vs Readiness:**
- **Liveness:** Is the process running? (Kubernetes restart trigger)
- **Readiness:** Can it serve traffic? (Kubernetes traffic routing)

**Adding Custom Health Checks:**
```csharp
builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database", tags: new[] { "ready" });
```

## Resilience Module

**Purpose:** Standard resilience patterns for HttpClient.

**Requires:** .NET 8.0 or later

**Usage:**
```csharp
builder.Services.AddHttpClient("MyApi")
    .AddXavierDefaults();
```

**Patterns Applied:**
- Retry with exponential backoff
- Circuit breaker
- Timeout
- Bulkhead isolation

## Observability Module

**Purpose:** OpenTelemetry integration for traces and metrics.

**Services Registered:**
- `AddOpenTelemetry()`
- ASP.NET Core instrumentation
- HttpClient instrumentation
- Runtime instrumentation (optional)

**Important:** Xavier does NOT auto-enable exporters. You must configure exporters via callbacks:

```csharp
builder.AddXavier(options =>
{
    options.Observability.Enabled = true;
    options.Observability.ServiceName = "MyApi";
    options.Observability.ConfigureTracing = tracing => 
        tracing.AddOtlpExporter();
    options.Observability.ConfigureMetrics = metrics => 
        metrics.AddOtlpExporter();
});
```

**Traces Collected:**
- ASP.NET Core requests
- HttpClient requests

**Metrics Collected:**
- ASP.NET Core metrics
- HttpClient metrics
- Runtime metrics (GC, threads, etc.) - optional

## Extension Points

### Adding Custom Modules

Xavier's internal design is modular. While not exposed publicly in v1, the internal architecture follows:

```csharp
internal interface IXavierModule
{
    void ConfigureServices(IServiceCollection services, XavierOptions options);
    void Configure(IApplicationBuilder app, XavierOptions options);
}
```

### Middleware Order

Xavier installs middleware in this order:
1. Correlation ID (early)
2. Exception handling / ProblemDetails
3. Rate limiting (early, before heavy processing)
4. HTTP logging (configurable position)
5. Response enrichment

## Disabling Modules

Disable any module by setting `Enabled = false`:

```json
{
  "Xavier": {
    "Health": { "Enabled": false },
    "OpenApi": { "Enabled": false }
  }
}
```

Or programmatically:
```csharp
builder.AddXavier(options =>
{
    options.Health.Enabled = false;
    options.OpenApi.Enabled = false;
});
```
