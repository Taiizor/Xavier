using Microsoft.OpenApi;
using Xavier.Extensions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add Swashbuckle services (following Microsoft docs: https://learn.microsoft.com/en-us/aspnet/core/tutorials/getting-started-with-swashbuckle)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Xavier Sample API",
        Version = "v1",
        Description = "A sample API demonstrating Xavier with Swagger UI",
        Contact = new OpenApiContact
        {
            Name = "Xavier Team",
            Url = new Uri("https://github.com/Taiizor/Xavier")
        },
        License = new OpenApiLicense
        {
            Name = "MIT",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
    });
});

// Add Xavier - correlation ID and health only
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

// Configure Swagger middleware (following Microsoft docs)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Xavier Sample API v1");
        options.RoutePrefix = "swagger";
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

// Home endpoint - redirect to Swagger UI
app.MapGet("/", () => Results.Redirect("/swagger"))
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
