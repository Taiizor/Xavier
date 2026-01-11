# Xavier Observability Sample

This sample demonstrates Xavier's OpenTelemetry integration for distributed tracing and metrics.

## Key Concept: Exporters Not Auto-Enabled

**Important:** Xavier does NOT auto-enable exporters. This is intentional per the project spec.

You must configure exporters via callbacks:

```csharp
builder.AddXavier(options =>
{
    options.Observability.Enabled = true;
    options.Observability.ServiceName = "MyApi";
    
    // Configure tracing exporter
    options.Observability.ConfigureTracing = tracing =>
        tracing.AddOtlpExporter(o => o.Endpoint = new Uri("http://localhost:4317"));
    
    // Configure metrics exporter  
    options.Observability.ConfigureMetrics = metrics =>
        metrics.AddOtlpExporter(o => o.Endpoint = new Uri("http://localhost:4317"));
});
```

## Features Demonstrated

- OpenTelemetry tracing with ASP.NET Core instrumentation
- OpenTelemetry metrics with ASP.NET Core instrumentation
- HTTP client distributed tracing
- Runtime metrics (GC, threads, etc.)
- Console and OTLP exporters

## Running the Sample

```bash
dotnet run
```

## Testing

### Basic Trace

```bash
curl http://localhost:5000/api/trace-demo
```

Watch the console for trace output.

### Slow Operation (Duration)

```bash
curl http://localhost:5000/api/slow
```

### Metrics

```bash
curl http://localhost:5000/api/metrics-demo
```

## Using with Jaeger

1. Start Jaeger:
   ```bash
   docker run -d --name jaeger \
     -p 16686:16686 \
     -p 4317:4317 \
     jaegertracing/all-in-one:latest
   ```

2. Uncomment the OTLP exporter in Program.cs

3. Run the sample and visit http://localhost:16686

## Configuration

```json
{
  "Xavier": {
    "Observability": {
      "Enabled": true,
      "ServiceName": "MyApi",
      "ServiceVersion": "1.0.0",
      "EnableTracing": true,
      "EnableMetrics": true,
      "EnableRuntimeMetrics": true
    }
  }
}
```

## Notes

- Exporters must be added via `ConfigureTracing`/`ConfigureMetrics` callbacks
- Xavier does NOT read exporter config from appsettings.json
- This is intentional to avoid forcing dependencies on specific exporters
