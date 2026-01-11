using Xavier.Extensions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add Xavier
builder.AddXavier();

// Option 1: Apply resilience defaults to ALL HttpClients
// builder.Services.AddXavierHttpClientDefaults();

// Option 2: Apply resilience to specific clients using AddXavierDefaults()
// This gives more control over which clients get resilience

// Client with Xavier resilience defaults (retry, timeout, circuit breaker)
builder.Services.AddHttpClient("ResilientClient", client =>
{
    client.BaseAddress = new Uri("https://httpbin.org");
})
.AddXavierDefaults(); // Adds standard resilience handler

// Client without resilience for comparison
builder.Services.AddHttpClient("BasicClient", client =>
{
    client.BaseAddress = new Uri("https://httpbin.org");
});

WebApplication app = builder.Build();

// Use Xavier middleware
app.UseXavier();

// Map infrastructure endpoints
app.MapXavierInfrastructureEndpoints();

app.MapGet("/", () => "Xavier Resilience Sample - Test retry, timeout, and circuit breaker patterns!");

// Endpoint demonstrating successful resilient request
app.MapGet("/api/resilient-success", async (IHttpClientFactory clientFactory) =>
{
    HttpClient client = clientFactory.CreateClient("ResilientClient");

    try
    {
        HttpResponseMessage response = await client.GetAsync("/get");
        string content = await response.Content.ReadAsStringAsync();
        return Results.Ok(new
        {
            Status = "Success",
            ClientType = "ResilientClient (with Xavier defaults)",
            HttpStatus = response.StatusCode.ToString(),
            Message = "Request succeeded with resilience patterns applied"
        });
    }
    catch (Exception ex)
    {
        return Results.Problem(
            title: "Request failed",
            detail: ex.Message,
            statusCode: 500
        );
    }
});

// Endpoint demonstrating retry behavior with flaky endpoint
app.MapGet("/api/resilient-retry", async (IHttpClientFactory clientFactory) =>
{
    HttpClient client = clientFactory.CreateClient("ResilientClient");

    try
    {
        // This endpoint returns 500 50% of the time
        HttpResponseMessage response = await client.GetAsync("/status/500");
        return Results.Ok(new
        {
            Status = "Completed",
            HttpStatus = response.StatusCode.ToString(),
            Message = "Retry policy may have kicked in if initial requests failed"
        });
    }
    catch (Exception ex)
    {
        return Results.Ok(new
        {
            Status = "Failed after retries",
            Error = ex.Message,
            Message = "Request failed even after retry attempts"
        });
    }
});

// Endpoint demonstrating timeout behavior
app.MapGet("/api/resilient-timeout", async (IHttpClientFactory clientFactory) =>
{
    HttpClient client = clientFactory.CreateClient("ResilientClient");

    try
    {
        // This endpoint delays response by 5 seconds
        // Standard resilience has a 30-second timeout by default
        HttpResponseMessage response = await client.GetAsync("/delay/5");
        return Results.Ok(new
        {
            Status = "Success",
            HttpStatus = response.StatusCode.ToString(),
            Message = "Delayed request completed within timeout"
        });
    }
    catch (TaskCanceledException)
    {
        return Results.Ok(new
        {
            Status = "Timeout",
            Message = "Request was cancelled due to timeout policy"
        });
    }
    catch (Exception ex)
    {
        return Results.Problem(
            title: "Request failed",
            detail: ex.Message,
            statusCode: 500
        );
    }
});

// Compare with basic client (no resilience)
app.MapGet("/api/basic-request", async (IHttpClientFactory clientFactory) =>
{
    HttpClient client = clientFactory.CreateClient("BasicClient");

    try
    {
        HttpResponseMessage response = await client.GetAsync("/get");
        return Results.Ok(new
        {
            Status = "Success",
            ClientType = "BasicClient (no resilience)",
            HttpStatus = response.StatusCode.ToString(),
            Message = "Request succeeded but has no retry/timeout/circuit breaker"
        });
    }
    catch (Exception ex)
    {
        return Results.Problem(
            title: "Request failed",
            detail: ex.Message,
            statusCode: 500
        );
    }
});

app.Run();
