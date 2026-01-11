using Xavier.Extensions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add Xavier with HTTP logging enabled
builder.AddXavier(options =>
{
    // Enable HTTP logging (disabled by default for security)
    options.HttpLogging.Enabled = true;

    // Log request/response headers (default: true)
    options.HttpLogging.LogRequestHeaders = true;
    options.HttpLogging.LogResponseHeaders = true;

    // Log request/response bodies (default: false for security)
    // Only enable in development for debugging!
    options.HttpLogging.LogRequestBody = builder.Environment.IsDevelopment();
    options.HttpLogging.LogResponseBody = builder.Environment.IsDevelopment();

    // Body log limits
    options.HttpLogging.RequestBodyLogLimit = 4096;
    options.HttpLogging.ResponseBodyLogLimit = 4096;

    // Configure excluded headers (security-sensitive headers)
    // These are excluded by default:
    // - Authorization
    // - Cookie
    // - Set-Cookie
    // - X-Api-Key
    // - X-Auth-Token

    // Add additional excluded headers if needed
    options.HttpLogging.ExcludedHeaders.Add("X-Custom-Secret");
});

WebApplication app = builder.Build();

// Use Xavier middleware (includes HTTP logging)
app.UseXavier();

// Map infrastructure endpoints
app.MapXavierInfrastructureEndpoints();

// Endpoints to demonstrate HTTP logging
app.MapGet("/", () => "Xavier HTTP Logging Sample - Check logs for request/response details!");

app.MapGet("/api/data", () => new
{
    Id = 1,
    Name = "Sample Data",
    Timestamp = DateTime.UtcNow,
    Message = "Check the console logs to see request/response logging"
});

app.MapPost("/api/echo", (EchoRequest request) => new
{
    Echo = request.Message,
    ReceivedAt = DateTime.UtcNow,
    Note = "Request body is logged if LogRequestBody is enabled"
});

app.MapGet("/api/secret", (HttpContext context) =>
{
    // Note: Authorization header is excluded from logs by default
    bool hasAuth = context.Request.Headers.ContainsKey("Authorization");
    return new
    {
        HasAuthHeader = hasAuth,
        Note = "Authorization header is NOT logged even if present"
    };
});

app.MapGet("/api/headers", (HttpContext context) =>
{
    // Return all headers (but sensitive ones won't be logged)
    var headers = context.Request.Headers
        .Select(h => new { Name = h.Key, Value = h.Value.ToString() })
        .ToList();

    return new
    {
        Headers = headers,
        Note = "Sensitive headers (Authorization, Cookie, etc.) are excluded from HTTP logs"
    };
});

app.Run();

record EchoRequest(string Message);
