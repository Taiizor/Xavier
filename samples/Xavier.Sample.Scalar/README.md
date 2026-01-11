# Xavier.Sample.Scalar

Demonstrates Scalar API Reference integration with Xavier using the latest NuGet packages.

## Overview

This sample shows how to use [Scalar.AspNetCore](https://github.com/scalar/scalar) - a modern API documentation UI that works with the latest `Microsoft.OpenApi` packages.

**Why Scalar?**
- ✅ Works with latest `Microsoft.OpenApi 3.1.2`
- ✅ Modern, beautiful UI with dark mode
- ✅ Code examples in multiple languages (C#, cURL, JavaScript, Python, etc.)
- ✅ No CDN required - works offline
- ✅ Fully compatible with .NET 10+

## Endpoints

| Endpoint | Description |
|----------|-------------|
| `/scalar/v1` | Scalar API Reference UI |
| `/openapi/v1.json` | OpenAPI 3.1.1 specification |
| `/api/products` | Product CRUD endpoints |
| `/api/categories` | Category listing |
| `/health` | Health check (liveness) |
| `/ready` | Readiness check |

## Running the Sample

```bash
cd samples/Xavier.Sample.Scalar
dotnet run
```

Then open http://localhost:5070/scalar/v1 in your browser.

## Features Demonstrated

1. **Scalar API Reference** - Modern API documentation UI
2. **ASP.NET Core built-in OpenAPI** - Native OpenAPI 3.1.1 spec generation
3. **Xavier Correlation ID** - Request tracking with `X-Correlation-Id` header
4. **Xavier Health Checks** - `/health` and `/ready` endpoints

## Package Versions

- `Microsoft.AspNetCore.OpenApi` - Latest (10.0.1)
- `Scalar.AspNetCore` - Latest (1.2.68)
- `Microsoft.OpenApi` - Latest (3.1.2)

## Comparison with Swashbuckle

| Feature | Scalar | Swashbuckle |
|---------|--------|-------------|
| OpenApi 3.x support | ✅ | ❌ (requires 2.x) |
| Dark mode | ✅ | ❌ |
| Code examples | ✅ Multiple languages | ❌ |
| Try-it-out | ✅ | ✅ |
| Search | ✅ | ✅ |
