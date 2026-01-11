# Xavier HTTP Logging Sample

This sample demonstrates Xavier's safe-by-default HTTP request/response logging.

## Features Demonstrated

- Request/response header logging
- Request/response body logging (opt-in)
- Automatic exclusion of sensitive headers
- Body log size limits

## Security: Excluded Headers

By default, Xavier excludes these sensitive headers from logs:

- `Authorization`
- `Cookie`
- `Set-Cookie`
- `X-Api-Key`
- `X-Auth-Token`

You can add additional exclusions:

```csharp
options.HttpLogging.ExcludedHeaders.Add("X-Custom-Secret");
```

## Running the Sample

```bash
dotnet run
```

## Testing

### Basic Request Logging

```bash
curl http://localhost:5000/api/data
```

Check the console for logged request/response details.

### Body Logging

```bash
curl -X POST http://localhost:5000/api/echo \
  -H "Content-Type: application/json" \
  -d '{"message": "Hello, Xavier!"}'
```

### Sensitive Header Exclusion

```bash
# Authorization header won't appear in logs
curl http://localhost:5000/api/secret \
  -H "Authorization: Bearer secret-token"
```

### All Headers

```bash
curl http://localhost:5000/api/headers \
  -H "X-Custom-Header: custom-value" \
  -H "Authorization: Bearer secret-token"
```

## Configuration

```json
{
  "Xavier": {
    "HttpLogging": {
      "Enabled": true,
      "LogRequestHeaders": true,
      "LogResponseHeaders": true,
      "LogRequestBody": false,
      "LogResponseBody": false,
      "RequestBodyLogLimit": 32768,
      "ResponseBodyLogLimit": 32768
    }
  },
  "Logging": {
    "LogLevel": {
      "Microsoft.AspNetCore.HttpLogging": "Information"
    }
  }
}
```

## Best Practices

1. **Never enable body logging in production** unless absolutely necessary
2. **Review excluded headers** to ensure all sensitive data is protected
3. **Set appropriate body log limits** to avoid memory issues
4. **Use environment-specific configuration** to enable body logging only in Development
