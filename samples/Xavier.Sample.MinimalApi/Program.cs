using Xavier.Extensions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add Xavier with one line!
builder.AddXavier(options =>
{
    // Optionally customize options
    options.HttpLogging.Enabled = true; // Enable HTTP logging
});

WebApplication app = builder.Build();

// Use Xavier middleware
app.UseXavier();

// Map infrastructure endpoints (health, OpenAPI)
app.MapXavierInfrastructureEndpoints();

// Your API endpoints
app.MapGet("/", () => "Hello from Xavier!");

app.MapGet("/api/users", () => new[]
{
    new { Id = 1, Name = "Alice" },
    new { Id = 2, Name = "Bob" }
});

app.MapGet("/api/users/{id}", (int id) =>
{
    if (id <= 0)
    {
        return Results.NotFound();
    }
    return Results.Ok(new { Id = id, Name = $"User {id}" });
});

app.MapPost("/api/users", (UserRequest request) =>
{
    return Results.Created($"/api/users/3", new { Id = 3, Name = request.Name });
});

// This will throw an exception to demonstrate ProblemDetails
app.MapGet("/api/error", () =>
{
    throw new InvalidOperationException("This is a test exception!");
});

app.Run();

record UserRequest(string Name);
