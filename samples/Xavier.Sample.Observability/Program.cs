using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Xavier.Extensions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add Xavier with observability enabled
// Note: Xavier does NOT auto-enable exporters. You must configure them via callbacks.
builder.AddXavier(options =>
{
    // Enable observability
    options.Observability.Enabled = true;
    options.Observability.ServiceName = "Xavier.Sample.Observability";
    options.Observability.ServiceVersion = "1.0.0";

    // Enable tracing and metrics
    options.Observability.EnableTracing = true;
    options.Observability.EnableMetrics = true;
    options.Observability.EnableRuntimeMetrics = true;

    // Configure tracing exporter via callback
    // Xavier does NOT auto-enable exporters - this is intentional per spec
    options.Observability.ConfigureTracing = tracing =>
    {
        // Console exporter for demo purposes
        tracing.AddConsoleExporter();

        // Uncomment to use OTLP exporter (e.g., for Jaeger, Zipkin, etc.)
        // tracing.AddOtlpExporter(otlp =>
        // {
        //     otlp.Endpoint = new Uri("http://localhost:4317");
        //     otlp.Protocol = OtlpExportProtocol.Grpc;
        // });
    };

    // Configure metrics exporter via callback
    options.Observability.ConfigureMetrics = metrics =>
    {
        // Console exporter for demo purposes
        metrics.AddConsoleExporter();

        // Uncomment to use OTLP exporter
        // metrics.AddOtlpExporter(otlp =>
        // {
        //     otlp.Endpoint = new Uri("http://localhost:4317");
        //     otlp.Protocol = OtlpExportProtocol.Grpc;
        // });
    };
});

// Add HttpClient to demonstrate distributed tracing
builder.Services.AddHttpClient("ExternalApi", client =>
{
    client.BaseAddress = new Uri("https://httpbin.org");
});

WebApplication app = builder.Build();

// Use Xavier middleware
app.UseXavier();

// Map infrastructure endpoints
app.MapXavierInfrastructureEndpoints();

// Endpoints to demonstrate tracing
app.MapGet("/", () => "Xavier Observability Sample - Check console for traces and metrics!");

app.MapGet("/api/trace-demo", async (IHttpClientFactory clientFactory) =>
{
    // This will create a span for the HTTP request
    HttpClient client = clientFactory.CreateClient("ExternalApi");

    // Make an external call - this will be traced
    try
    {
        HttpResponseMessage response = await client.GetAsync("/get");
        return Results.Ok(new
        {
            Message = "External API call completed",
            Status = response.StatusCode.ToString(),
            TraceInfo = "Check console for trace information"
        });
    }
    catch (Exception ex)
    {
        return Results.Ok(new
        {
            Message = "External API call failed (expected if offline)",
            Error = ex.Message,
            TraceInfo = "Check console for trace information"
        });
    }
});

app.MapGet("/api/slow", async () =>
{
    // Simulate slow operation to demonstrate duration tracking
    await Task.Delay(TimeSpan.FromSeconds(2));
    return new { Message = "Slow operation completed", DurationMs = 2000 };
});

app.MapGet("/api/metrics-demo", () =>
{
    // Simply returning data - metrics are collected automatically
    return new
    {
        Message = "Metrics are collected automatically",
        Info = "ASP.NET Core metrics, HTTP client metrics, and runtime metrics are all captured"
    };
});

app.Run();
