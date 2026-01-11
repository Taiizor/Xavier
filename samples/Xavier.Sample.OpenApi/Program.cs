using Xavier.Extensions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add Xavier with all defaults
builder.AddXavier(options =>
{
    // Enable OpenAPI (this is enabled by default, shown for clarity)
    options.OpenApi.Enabled = true;
    options.OpenApi.DocumentName = "v1";

    // Enable in non-development environments (default: false)
    // Uncomment to enable OpenAPI in production:
    // options.OpenApi.ExposeInNonDevelopment = true;
});

// Add built-in OpenAPI support (requires Microsoft.AspNetCore.OpenApi package)
builder.Services.AddOpenApi();

WebApplication app = builder.Build();

// Use Xavier middleware (correlation ID, error handling, etc.)
app.UseXavier();

// Map built-in OpenAPI endpoint (available in .NET 9+)
// This maps to /openapi/v1.json by default
app.MapOpenApi();

// Map Xavier infrastructure endpoints (health checks)
app.MapXavierInfrastructureEndpoints();

// Demo API endpoints
app.MapGet("/", () => "Xavier OpenAPI Sample - Visit /openapi/v1.json to see the OpenAPI document!");

app.MapGet("/api/products", () => new[]
{
    new Product(1, "Laptop", 999.99m, "Electronics"),
    new Product(2, "Keyboard", 79.99m, "Electronics"),
    new Product(3, "Monitor", 299.99m, "Electronics")
})
.WithName("GetProducts")
.WithDescription("Gets all products")
.WithTags("Products");

app.MapGet("/api/products/{id}", (int id) =>
{
    if (id <= 0)
    {
        return Results.NotFound(new { message = "Product not found" });
    }
    return Results.Ok(new Product(id, $"Product {id}", 99.99m, "General"));
})
.WithName("GetProductById")
.WithDescription("Gets a product by its ID")
.WithTags("Products");

app.MapPost("/api/products", (ProductCreateRequest request) =>
{
    Product newProduct = new(new Random().Next(100, 1000), request.Name, request.Price, request.Category);
    return Results.Created($"/api/products/{newProduct.Id}", newProduct);
})
.WithName("CreateProduct")
.WithDescription("Creates a new product")
.WithTags("Products");

app.MapGet("/api/categories", () => new[] { "Electronics", "Clothing", "Food", "General" })
.WithName("GetCategories")
.WithDescription("Gets all product categories")
.WithTags("Categories");

// Error endpoint to demonstrate ProblemDetails
app.MapGet("/api/error", () =>
{
    throw new InvalidOperationException("This is a test exception to demonstrate ProblemDetails error handling!");
})
.WithName("SimulateError")
.WithDescription("Simulates an error to show ProblemDetails response")
.WithTags("Debug");

app.Run();

// Models
record Product(int Id, string Name, decimal Price, string Category);
record ProductCreateRequest(string Name, decimal Price, string Category);
