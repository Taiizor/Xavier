# Xavier.Sample.OpenApi

This sample demonstrates using Xavier with .NET's built-in OpenAPI support (available in .NET 9+).

## Features Demonstrated

- Built-in OpenAPI document generation (`/openapi/v1.json`)
- Xavier infrastructure endpoints (health checks)
- ProblemDetails error handling with correlation ID
- Endpoint metadata (names, descriptions, tags)

## Prerequisites

- .NET 10 or later (also works with .NET 9+)
- Microsoft.AspNetCore.OpenApi package (included in project)

## Running the Sample

```bash
cd samples/Xavier.Sample.OpenApi
dotnet run
```

Then visit:
- http://localhost:5050/ - Home page
- http://localhost:5050/openapi/v1.json - OpenAPI document
- http://localhost:5050/api/products - Get all products
- http://localhost:5050/api/products/1 - Get product by ID
- http://localhost:5050/api/categories - Get categories
- http://localhost:5050/api/error - Simulate error (shows ProblemDetails)
- http://localhost:5050/health - Liveness check
- http://localhost:5050/ready - Readiness check

## OpenAPI Configuration

```csharp
builder.AddXavier(options =>
{
    options.OpenApi.Enabled = true;
    options.OpenApi.DocumentName = "v1";
    // options.OpenApi.ExposeInNonDevelopment = true; // Enable in production
});

// Register OpenAPI services
builder.Services.AddOpenApi();

// Map OpenAPI endpoint
app.MapOpenApi();
```

## Notes

- OpenAPI is disabled by default in non-Development environments for security
- Set `ExposeInNonDevelopment = true` to enable in production
- Use Swagger UI, Scalar, or other tools to visualize the OpenAPI document
