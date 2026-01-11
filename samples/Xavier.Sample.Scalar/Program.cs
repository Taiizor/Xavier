using Scalar.AspNetCore;
using Xavier.Extensions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add built-in OpenAPI support (Microsoft.AspNetCore.OpenApi)
builder.Services.AddOpenApi("v1", options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info.Title = "Xavier Sample API";
        document.Info.Version = "v1";
        document.Info.Description = "A sample API demonstrating Xavier with Scalar API Reference";
        document.Info.Contact = new Microsoft.OpenApi.OpenApiContact
        {
            Name = "Xavier Team",
            Url = new Uri("https://github.com/Taiizor/Xavier")
        };
        document.Info.License = new Microsoft.OpenApi.OpenApiLicense
        {
            Name = "MIT",
            Url = new Uri("https://opensource.org/licenses/MIT")
        };
        return Task.CompletedTask;
    });
});

// Add Xavier - correlation ID only, disable other features to avoid interference
builder.AddXavier(options =>
{
    options.Correlation.Enabled = true;
    options.ErrorHandling.Enabled = false;
    options.HttpLogging.Enabled = false;
    options.RateLimiting.Enabled = false;
    options.OpenApi.Enabled = false;
    options.Health.Enabled = true;
    options.Observability.Enabled = false;
});

WebApplication app = builder.Build();

// Map built-in OpenAPI endpoint - generates spec at /openapi/v1.json
app.MapOpenApi();

// Map Scalar API Reference UI at /scalar (modern Swagger UI alternative)
// This uses the NuGet package Scalar.AspNetCore which is fully compatible with latest Microsoft.OpenApi
if (app.Environment.IsDevelopment())
{
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Xavier Sample API");
        options.WithTheme(ScalarTheme.Purple);
        options.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

// Use Xavier middleware
app.UseXavier();

// Map Xavier infrastructure endpoints (health checks)
app.MapXavierInfrastructureEndpoints();

// Demo API endpoints
RouteGroupBuilder productsGroup = app.MapGroup("/api/products")
    .WithTags("Products");

productsGroup.MapGet("/", () => new[]
{
    new Product(1, "Laptop", 999.99m, "Electronics", 50),
    new Product(2, "Keyboard", 79.99m, "Electronics", 200),
    new Product(3, "Monitor", 299.99m, "Electronics", 75)
})
.WithName("GetAllProducts")
.WithDescription("Retrieves all products from the catalog");

productsGroup.MapGet("/{id:int}", (int id) =>
{
    if (id <= 0)
    {
        return Results.NotFound(new { message = "Product not found", productId = id });
    }
    return Results.Ok(new Product(id, $"Product {id}", 99.99m, "General", 10));
})
.WithName("GetProductById")
.WithDescription("Retrieves a specific product by its unique identifier");

productsGroup.MapPost("/", (ProductCreateRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.Name))
    {
        return Results.BadRequest(new { message = "Product name is required" });
    }

    Product newProduct = new(
        new Random().Next(100, 1000),
        request.Name,
        request.Price,
        request.Category ?? "General",
        request.Stock ?? 0);

    return Results.Created($"/api/products/{newProduct.Id}", newProduct);
})
.WithName("CreateProduct")
.WithDescription("Creates a new product in the catalog");

productsGroup.MapPut("/{id:int}", (int id, ProductUpdateRequest request) =>
{
    if (id <= 0)
    {
        return Results.NotFound();
    }

    Product updatedProduct = new(id, request.Name, request.Price, request.Category, request.Stock);
    return Results.Ok(updatedProduct);
})
.WithName("UpdateProduct")
.WithDescription("Updates an existing product");

productsGroup.MapDelete("/{id:int}", (int id) =>
{
    if (id <= 0)
    {
        return Results.NotFound();
    }
    return Results.NoContent();
})
.WithName("DeleteProduct")
.WithDescription("Deletes a product from the catalog");

// Categories endpoints
RouteGroupBuilder categoriesGroup = app.MapGroup("/api/categories")
    .WithTags("Categories");

categoriesGroup.MapGet("/", () => new[]
{
    new Category(1, "Electronics", "Electronic devices and accessories"),
    new Category(2, "Clothing", "Apparel and fashion items"),
    new Category(3, "Food", "Food and beverages"),
    new Category(4, "General", "General merchandise")
})
.WithName("GetAllCategories")
.WithDescription("Retrieves all product categories");

// Home endpoint - redirect to Scalar API Reference
app.MapGet("/", () => Results.Redirect("/scalar/v1"))
    .ExcludeFromDescription();

// Error endpoint to demonstrate ProblemDetails
app.MapGet("/api/error", () =>
{
    throw new InvalidOperationException("This is a test exception to demonstrate error handling!");
})
.WithName("SimulateError")
.WithDescription("Simulates an error to demonstrate error handling")
.WithTags("Debug");

app.Run();

// Models
record Product(int Id, string Name, decimal Price, string Category, int Stock);
record Category(int Id, string Name, string Description);
record ProductCreateRequest(string Name, decimal Price, string? Category, int? Stock);
record ProductUpdateRequest(string Name, decimal Price, string Category, int Stock);
