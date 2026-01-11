# Xavier Resilience Sample

This sample demonstrates Xavier's HttpClient resilience patterns using .NET 8+ standard resilience.

## Features Demonstrated

- Standard resilience handler (retry, timeout, circuit breaker)
- Per-client resilience configuration
- Comparison between resilient and basic clients

## Running the Sample

```bash
dotnet run
```

## Testing

### Successful Request with Resilience

```bash
curl http://localhost:5000/api/resilient-success
```

### Retry Behavior

```bash
# This may trigger retries due to 500 status codes
curl http://localhost:5000/api/resilient-retry
```

### Timeout Behavior

```bash
# Tests timeout handling (5 second delay)
curl http://localhost:5000/api/resilient-timeout
```

### Basic Client (No Resilience)

```bash
curl http://localhost:5000/api/basic-request
```

## Usage Patterns

### Option 1: Apply to All Clients

```csharp
builder.Services.AddXavierHttpClientDefaults();
```

### Option 2: Apply to Specific Clients

```csharp
builder.Services.AddHttpClient("MyApi")
    .AddXavierDefaults();
```

## Standard Resilience Patterns

Xavier's standard resilience includes:

1. **Retry** - Exponential backoff retry policy
2. **Circuit Breaker** - Prevents cascade failures
3. **Timeout** - Request timeout protection
4. **Bulkhead** - Limits concurrent requests

## Configuration

```json
{
  "Xavier": {
    "Resilience": {
      "Enabled": true,
      "UseStandardResilience": true,
      "MaxRetryAttempts": 3,
      "TimeoutSeconds": 30
    }
  }
}
```

## Notes

- Standard resilience requires .NET 8 or later
- On earlier TFMs, `AddXavierDefaults()` is a no-op (warning logged)
- Uses `Microsoft.Extensions.Http.Resilience` under the hood
