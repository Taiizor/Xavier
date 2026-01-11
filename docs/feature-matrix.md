# Xavier Feature Matrix

This document shows feature availability across different .NET Target Framework Monikers (TFMs).

## Feature Support by TFM

| Feature                  | net6.0 | net7.0 | net8.0 | net9.0 | net10.0 | Notes                                    |
| ------------------------ | :----: | :----: | :----: | :----: | :-----: | ---------------------------------------- |
| Error Handling           |   ✅   |   ✅   |   ✅   |   ✅   |   ✅    | ProblemDetails service available net7.0+ |
| ProblemDetails Service   |   ❌   |   ✅   |   ✅   |   ✅   |   ✅    | `AddProblemDetails()` requires net7.0+   |
| Correlation ID           |   ✅   |   ✅   |   ✅   |   ✅   |   ✅    | Full support all TFMs                    |
| HTTP Logging             |   ✅   |   ✅   |   ✅   |   ✅   |   ✅    | Duration field available net8.0+         |
| Rate Limiting            |   ❌   |   ✅   |   ✅   |   ✅   |   ✅    | Requires net7.0+                         |
| OpenAPI (built-in)       |   ❌   |   ❌   |   ⚠️   |   ✅   |   ✅    | MapOpenApi requires net9.0+              |
| Health Endpoints         |   ✅   |   ✅   |   ✅   |   ✅   |   ✅    | Full support all TFMs                    |
| HTTP Resilience          |   ❌   |   ❌   |   ✅   |   ✅   |   ✅    | Standard resilience requires net8.0+     |
| Observability (OTel)     |   ✅   |   ✅   |   ✅   |   ✅   |   ✅    | Basic support all TFMs                   |

### Legend

- ✅ = Fully supported
- ⚠️ = Partially supported (see notes)
- ❌ = Not available (graceful no-op, warning logged)

## Unsupported Feature Behavior

When a feature is enabled in configuration but not supported on the current TFM:

1. Xavier **does not crash** at runtime
2. The feature is **automatically disabled**
3. A **single warning log** is emitted per process (not per request)
4. The application continues running normally

## Feature Details

### Error Handling

- **net6.0**: Basic exception handling with `UseExceptionHandler()` and `UseStatusCodePages()`
- **net7.0+**: Full `AddProblemDetails()` service with customization support

### Rate Limiting

- **net6.0**: No-op (warning logged once)
- **net7.0+**: Full `System.Threading.RateLimiting` support with fixed window policies

### OpenAPI

- **net6.0-net7.0**: No built-in OpenAPI support
- **net8.0**: Configuration available but `MapOpenApi()` not present
- **net9.0+**: Full built-in OpenAPI with `MapOpenApi()`

Note: Xavier does NOT ship SwaggerUI. If you need a UI, add it separately.

### HTTP Resilience

- **net6.0-net7.0**: No-op (warning logged once)
- **net8.0+**: Full `Microsoft.Extensions.Http.Resilience` support

## TFM Support Policy

| .NET Version | Upstream Status | Xavier Support Status     |
| ------------ | --------------- | ------------------------- |
| .NET 6       | Out of Support  | Supported (author choice) |
| .NET 7       | Out of Support  | Supported                 |
| .NET 8       | LTS             | Supported                 |
| .NET 9       | STS             | Supported                 |
| .NET 10      | Current/Preview | Supported                 |

Note: .NET 6 is out-of-support upstream, but Xavier still builds and runs on net6.0 because the author explicitly wants it.
