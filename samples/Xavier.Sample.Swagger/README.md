# Xavier.Sample.Swagger

Demonstrates Swashbuckle/Swagger UI integration with Xavier following [Microsoft's official documentation](https://learn.microsoft.com/en-us/aspnet/core/tutorials/getting-started-with-swashbuckle).

## Overview

This sample shows how to integrate Swagger UI using `Swashbuckle.AspNetCore` - the traditional approach for API documentation in ASP.NET Core.

## Endpoints

| Endpoint                   | Description             |
| -------------------------- | ----------------------- |
| `/swagger`                 | Swagger UI              |
| `/swagger/v1/swagger.json` | OpenAPI specification   |
| `/api/products`            | Product CRUD endpoints  |
| `/api/categories`          | Category listing        |
| `/health`                  | Health check (liveness) |
| `/ready`                   | Readiness check         |

## Running the Sample

```bash
cd samples/Xavier.Sample.Swagger
dotnet run
```

Then open http://localhost:5065/swagger in your browser.

## Features Demonstrated

1. **Swagger UI** - Interactive API documentation
2. **Swashbuckle.AspNetCore** - OpenAPI spec generation
3. **Xavier Correlation ID** - Request tracking with `X-Correlation-Id` header
4. **Xavier Health Checks** - `/health` and `/ready` endpoints

## Configuration

The Swagger setup follows Microsoft's recommended pattern:

```csharp
// Add services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "My API",
        Version = "v1"
    });
});

// Use middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
```

## Package Versions

This sample uses pinned package versions due to compatibility requirements:

-   `Swashbuckle.AspNetCore` - **10.1.0** (from Directory.Packages.props)
-   `Microsoft.OpenApi` - **2.3.0** (pinned via VersionOverride - required for Swashbuckle 10.x)

> **Note:** Swashbuckle.AspNetCore 10.x is **NOT compatible** with Microsoft.OpenApi 3.x. The project uses `VersionOverride="2.3.0"` to ensure compatibility since the solution's central package management uses OpenApi 3.x for other projects.

## Troubleshooting

### "ERR_UNSAFE_PORT" Browser Error

Port 5060 is blocked by browsers as an "unsafe port" (used for SIP protocol). The sample uses port **5065** instead.

### "Method not found: IOpenApiRequestBody.get_Content()" Exception

This error occurs when `Microsoft.OpenApi` 3.x is loaded instead of 2.x. Swashbuckle 10.x requires OpenApi 2.x.

**Solution:**

1. Make sure the project has `VersionOverride="2.3.0"` for `Microsoft.OpenApi`
2. Clean and rebuild:
    ```bash
    cd samples/Xavier.Sample.Swagger
    rm -rf bin obj
    dotnet restore
    dotnet build
    ```

### "Unable to render this definition" Error

1. Make sure packages are restored correctly:

    ```bash
    dotnet restore
    dotnet build
    ```

2. Verify `Microsoft.OpenApi` 2.3.0 is being used (not 3.x)

## Alternative: Scalar

For a modern alternative that works with the latest `Microsoft.OpenApi 3.x` packages, see the `Xavier.Sample.Scalar` project which uses [Scalar.AspNetCore](https://github.com/scalar/scalar).
