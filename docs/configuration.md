# Xavier Configuration Guide

Xavier is configured through the `Xavier` section in your application configuration (appsettings.json) and/or programmatically via the options pattern.

## Configuration Methods

### 1. appsettings.json (recommended for most settings)

```json
{
  "Xavier": {
    "Correlation": {
      "Enabled": true,
      "HeaderName": "X-Correlation-Id",
      "UseTraceparentFallback": false
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
      "DocumentName": "v1",
      "ExposeInNonDevelopment": false
    },
    "Health": {
      "Enabled": true,
      "LivenessPath": "/health",
      "ReadinessPath": "/ready"
    },
    "Resilience": {
      "Enabled": false
    },
    "Observability": {
      "Enabled": false
    }
  }
}
```

### 2. Programmatic Configuration

```csharp
builder.AddXavier(options =>
{
    options.Correlation.HeaderName = "X-Request-Id";
    options.ErrorHandling.IncludeExceptionDetails = builder.Environment.IsDevelopment();
    options.HttpLogging.Enabled = true;
    options.RateLimiting.Enabled = true;
    options.Observability.Enabled = true;
    options.Observability.ServiceName = "MyApi";
});
```

### 3. Both (appsettings + programmatic)

Programmatic configuration is applied after appsettings.json binding, so it can override config file values.

## Options Reference

### XavierOptions (root)

| Property       | Type                  | Default |
| -------------- | --------------------- | ------- |
| ErrorHandling  | ErrorHandlingOptions  | Enabled |
| Correlation    | CorrelationOptions    | Enabled |
| HttpLogging    | HttpLoggingOptions    | Disabled |
| RateLimiting   | RateLimitingOptions   | Disabled |
| OpenApi        | OpenApiOptions        | Enabled |
| Health         | HealthOptions         | Enabled |
| Resilience     | ResilienceOptions     | Disabled |
| Observability  | ObservabilityOptions  | Disabled |

### CorrelationOptions

| Property         | Type   | Default           | Description                            |
| ---------------- | ------ | ----------------- | -------------------------------------- |
| Enabled          | bool   | true              | Enable correlation ID feature          |
| HeaderName       | string | "X-Correlation-Id"| Header name for correlation ID         |
| AddToResponse    | bool   | true              | Add correlation ID to response headers |
| UseTraceparent   | bool   | false             | Use W3C traceparent header as fallback |
| EnrichLogScopes  | bool   | true              | Enrich log scopes with correlation data |

### ErrorHandlingOptions

| Property               | Type | Default | Description                           |
| ---------------------- | ---- | ------- | ------------------------------------- |
| Enabled                | bool | true    | Enable error handling                 |
| IncludeExceptionDetails| bool | false   | Include exception details in response |
| IncludeTraceId         | bool | true    | Include trace ID in ProblemDetails    |
| IncludeCorrelationId   | bool | true    | Include correlation ID in ProblemDetails |

### HttpLoggingOptions

| Property            | Type         | Default   | Description                        |
| ------------------- | ------------ | --------- | ---------------------------------- |
| Enabled             | bool         | false     | Enable HTTP logging (opt-in)       |
| LogRequestHeaders   | bool         | true      | Log request headers                |
| LogResponseHeaders  | bool         | true      | Log response headers               |
| LogRequestBody      | bool         | false     | Log request body                   |
| LogResponseBody     | bool         | false     | Log response body                  |
| ExcludedHeaders     | List<string> | See below | Headers excluded from logging      |
| RequestBodyLogLimit | int          | 32768     | Max request body bytes to log      |
| ResponseBodyLogLimit| int          | 32768     | Max response body bytes to log     |

**Default ExcludedHeaders:** Authorization, Cookie, Set-Cookie, X-Api-Key, X-Auth-Token

### RateLimitingOptions

| Property            | Type   | Default      | Description                     |
| ------------------- | ------ | ------------ | ------------------------------- |
| Enabled             | bool   | false        | Enable rate limiting (opt-in)   |
| EnableGlobalPolicy  | bool   | true         | Enable global rate limit policy |
| EnablePerIpPolicy   | bool   | true         | Enable per-IP rate limit policy |
| EnablePerClientPolicy| bool  | false        | Enable per-client rate limit policy |
| ClientIdHeader      | string | "X-Client-Id"| Header for client identification |
| PermitLimit         | int    | 100          | Requests allowed per window     |
| WindowSeconds       | int    | 60           | Window duration in seconds      |
| QueueLimit          | int    | 0            | Queue limit for excess requests |
| AddRateLimitHeaders | bool   | true         | Add rate limit headers to rate-limited endpoints |

**Rate Limit Headers:** When `AddRateLimitHeaders` is enabled, headers are added only to endpoints with rate limiting policies (`.RequireRateLimiting()`):
- `X-RateLimit-Limit`: Maximum requests allowed
- `X-RateLimit-Remaining`: Remaining requests in window (decreases with each request)
- `X-RateLimit-Used`: Requests used in current window (increases with each request)
- `X-RateLimit-Reset`: Unix timestamp when limit resets (consistent within same window)
- `Retry-After`: Seconds to wait (only on 429 responses)

**Important:** Each rate limiting policy has its own separate counter. Requests to endpoints without rate limiting do not count towards any limit.

### OpenApiOptions

| Property              | Type   | Default                       | Description                    |
| --------------------- | ------ | ----------------------------- | ------------------------------ |
| Enabled               | bool   | true                          | Enable OpenAPI                 |
| DocumentName          | string | "v1"                          | OpenAPI document name          |
| PathTemplate          | string | "/openapi/{documentName}.json"| Path template for document     |
| ExposeInNonDevelopment| bool   | false                         | Expose outside Development env |
| Title                 | string | null                          | Document title                 |
| Version               | string | null                          | Document version               |

### HealthOptions

| Property       | Type         | Default   | Description                      |
| -------------- | ------------ | --------- | -------------------------------- |
| Enabled        | bool         | true      | Enable health endpoints          |
| LivenessPath   | string       | "/health" | Liveness check path              |
| ReadinessPath  | string       | "/ready"  | Readiness check path             |
| ReadinessTags  | List<string> | ["ready"] | Tags to filter readiness checks  |
| IncludeDetails | bool         | false     | Include detailed health info     |

### ResilienceOptions

| Property            | Type | Default | Description                      |
| ------------------- | ---- | ------- | -------------------------------- |
| Enabled             | bool | false   | Enable resilience (opt-in)       |
| UseStandardResilience| bool| true    | Use standard resilience handler  |
| MaxRetryAttempts    | int  | 3       | Maximum retry attempts           |
| TimeoutSeconds      | int  | 30      | Request timeout in seconds       |

### ObservabilityOptions

| Property           | Type                         | Default | Description                    |
| ------------------ | ---------------------------- | ------- | ------------------------------ |
| Enabled            | bool                         | false   | Enable observability (opt-in)  |
| ServiceName        | string                       | null    | Service name for telemetry     |
| ServiceVersion     | string                       | null    | Service version for telemetry  |
| EnableTracing      | bool                         | true    | Enable distributed tracing     |
| EnableMetrics      | bool                         | true    | Enable metrics                 |
| EnableRuntimeMetrics| bool                        | true    | Enable runtime metrics         |
| ConfigureTracing   | Action<TracerProviderBuilder>| null    | Callback to configure tracing  |
| ConfigureMetrics   | Action<MeterProviderBuilder> | null    | Callback to configure metrics  |

**Note:** Xavier does NOT auto-enable exporters. Use the callback properties to add exporters:

```csharp
builder.AddXavier(options =>
{
    options.Observability.Enabled = true;
    options.Observability.ServiceName = "MyApi";
    options.Observability.ConfigureTracing = tracing => 
        tracing.AddOtlpExporter(o => o.Endpoint = new Uri("http://localhost:4317"));
    options.Observability.ConfigureMetrics = metrics => 
        metrics.AddOtlpExporter(o => o.Endpoint = new Uri("http://localhost:4317"));
});
```

## Environment-specific Configuration

Use environment-specific appsettings files:

**appsettings.Development.json:**
```json
{
  "Xavier": {
    "ErrorHandling": {
      "IncludeExceptionDetails": true
    },
    "HttpLogging": {
      "Enabled": true
    }
  }
}
```

**appsettings.Production.json:**
```json
{
  "Xavier": {
    "ErrorHandling": {
      "IncludeExceptionDetails": false
    },
    "RateLimiting": {
      "Enabled": true,
      "PermitLimit": 1000
    }
  }
}
```

## Security Best Practices

1. **Never enable `IncludeExceptionDetails` in production** - it can leak sensitive information
2. **Keep HTTP body logging disabled** unless absolutely necessary
3. **Enable rate limiting in production** to prevent abuse
4. **Keep OpenAPI disabled in production** or use authentication
5. **Review excluded headers** to ensure sensitive data is not logged
