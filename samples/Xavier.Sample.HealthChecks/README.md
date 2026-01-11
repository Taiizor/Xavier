# Xavier Health Checks Sample

This sample demonstrates Xavier's health check endpoints for Kubernetes/Docker orchestration.

## Features Demonstrated

- Liveness endpoint (`/health`)
- Readiness endpoint (`/ready`)
- Custom health checks
- Health check tagging for readiness
- Detailed health information in responses

## Health Check Concepts

### Liveness vs Readiness

| Endpoint | Purpose | When to use |
|----------|---------|-------------|
| `/health` (Liveness) | Is the process running? | Kubernetes restarts if unhealthy |
| `/ready` (Readiness) | Can it serve traffic? | Kubernetes routes traffic only when ready |

### Xavier's Default Behavior

- **Liveness** (`/health`): Always returns healthy if app is running (no checks)
- **Readiness** (`/ready`): Runs checks tagged with "ready"

## Running the Sample

```bash
dotnet run
```

## Testing

### Liveness Check

```bash
curl http://localhost:5000/health
```

Returns 200 OK if the application is running.

### Readiness Check

```bash
curl http://localhost:5000/ready
```

Returns 200 OK if all readiness checks pass.

### Detailed Health Information

With `IncludeDetails = true`, responses include JSON details:

```json
{
  "status": "Healthy",
  "results": {
    "database": {
      "status": "Healthy",
      "description": "Database connection is healthy"
    },
    "external-api": {
      "status": "Healthy", 
      "description": "External API is reachable"
    }
  }
}
```

## Adding Custom Health Checks

```csharp
builder.Services.AddHealthChecks()
    .AddCheck<MyHealthCheck>("my-check", tags: new[] { "ready" });
```

Health checks tagged with "ready" are included in the readiness probe.

## Kubernetes Integration

```yaml
apiVersion: v1
kind: Pod
spec:
  containers:
  - name: myapp
    livenessProbe:
      httpGet:
        path: /health
        port: 80
      initialDelaySeconds: 5
      periodSeconds: 10
    readinessProbe:
      httpGet:
        path: /ready
        port: 80
      initialDelaySeconds: 5
      periodSeconds: 10
```

## Configuration

```json
{
  "Xavier": {
    "Health": {
      "Enabled": true,
      "LivenessPath": "/health",
      "ReadinessPath": "/ready",
      "IncludeDetails": true,
      "ReadinessTags": ["ready"]
    }
  }
}
```

## Notes

- Liveness endpoint runs NO checks (always healthy if app responds)
- Readiness endpoint only runs checks tagged with specified tags
- Set `IncludeDetails = false` in production for security
