# Xavier Rate Limiting Sample

This sample demonstrates Xavier's rate limiting feature using .NET 7+ built-in rate limiting.

## Features Demonstrated

- Global rate limiting policy
- Per-IP rate limiting policy  
- Per-client rate limiting policy (using `X-Client-Id` header)
- Rate limit response headers on rate-limited endpoints only
- 429 Too Many Requests responses with JSON body
- Consistent window reset timestamps
- Separate counters for each rate limiting policy

## Rate Limit Response Headers

Xavier adds rate limit headers **only to endpoints with rate limiting enabled** (endpoints using `.RequireRateLimiting()`).

| Header | Description | Example |
|--------|-------------|---------|
| `X-RateLimit-Limit` | Maximum requests allowed in the window | `10` |
| `X-RateLimit-Remaining` | Remaining requests in current window | `9` |
| `X-RateLimit-Used` | Requests used in current window | `1` |
| `X-RateLimit-Reset` | Unix timestamp when the rate limit resets | `1767434620` |
| `Retry-After` | Seconds to wait before retrying (only on 429) | `45` |

**Note:** Endpoints without `.RequireRateLimiting()` will NOT have rate limit headers and will NOT count towards the rate limit.

**Success Response Headers (200 OK):**
```
HTTP/1.1 200 OK
X-RateLimit-Limit: 10
X-RateLimit-Remaining: 9
X-RateLimit-Used: 1
X-RateLimit-Reset: 1767434620
```

After 5 requests to the **same rate-limited endpoint**:
```
HTTP/1.1 200 OK
X-RateLimit-Limit: 10
X-RateLimit-Remaining: 5
X-RateLimit-Used: 5
X-RateLimit-Reset: 1767434620
```

**Rate Limited Response Headers (429):**
```
HTTP/1.1 429 Too Many Requests
X-RateLimit-Limit: 10
X-RateLimit-Remaining: 0
X-RateLimit-Used: 10
X-RateLimit-Reset: 1767434620
Retry-After: 45
```

### Policy-Based Counting

Each rate limiting policy has its own counter:
- `/api/data` with `GlobalPolicyName` has its own counter
- `/api/per-ip` with `PerIpPolicyName` has a separate counter per IP
- `/api/per-client` with `PerClientPolicyName` has a separate counter per client

Hitting one endpoint does NOT affect the count for other endpoints.

### Reset Time Consistency

The `X-RateLimit-Reset` timestamp uses fixed window calculation:
- The reset time represents when the current rate limit window ends
- Multiple requests within the same window get the **same** reset timestamp
- Reset time is calculated as: `(current_time / window_seconds) * window_seconds + window_seconds`

## Running the Sample

```bash
dotnet run
```

## Testing Rate Limiting

### Check Rate Limit Headers on Success

```bash
# Check headers with verbose output - headers appear on ALL requests
curl -v http://localhost:5000/api/data 2>&1 | grep -i ratelimit
```

### Global Rate Limit

```bash
# Send multiple requests quickly to see rate limiting in action
for i in {1..15}; do curl -s -o /dev/null -w "%{http_code}\n" http://localhost:5000/api/data; done
```

### Per-IP Rate Limit

```bash
# Each IP gets its own rate limit bucket
for i in {1..15}; do curl -s -o /dev/null -w "%{http_code}\n" http://localhost:5000/api/per-ip; done
```

### Per-Client Rate Limit

```bash
# Different clients get different rate limit buckets
for i in {1..15}; do curl -s -o /dev/null -w "%{http_code}\n" -H "X-Client-Id: client-1" http://localhost:5000/api/per-client; done

# Different client ID = different bucket
for i in {1..15}; do curl -s -o /dev/null -w "%{http_code}\n" -H "X-Client-Id: client-2" http://localhost:5000/api/per-client; done
```

### View 429 Response

```bash
# Exceed the limit and see the JSON error response
for i in {1..12}; do curl -s http://localhost:5000/api/data; done
```

Example 429 response:
```json
{
  "type": "https://tools.ietf.org/html/rfc6585#section-4",
  "title": "Too Many Requests",
  "status": 429,
  "detail": "Rate limit exceeded. Please try again later."
}
```

## Configuration

Configure rate limiting via `appsettings.json`:

```json
{
  "Xavier": {
    "RateLimiting": {
      "Enabled": true,
      "EnableGlobalPolicy": true,
      "EnablePerIpPolicy": true,
      "EnablePerClientPolicy": true,
      "PermitLimit": 10,
      "WindowSeconds": 60,
      "QueueLimit": 0,
      "AddRateLimitHeaders": true,
      "ClientIdHeader": "X-Client-Id"
    }
  }
}
```

## Configuration Options

| Option | Default | Description |
|--------|---------|-------------|
| `Enabled` | `false` | Enable rate limiting |
| `EnableGlobalPolicy` | `true` | Enable global rate limit policy |
| `EnablePerIpPolicy` | `true` | Enable per-IP rate limit policy |
| `EnablePerClientPolicy` | `false` | Enable per-client rate limit policy |
| `PermitLimit` | `100` | Requests allowed per window |
| `WindowSeconds` | `60` | Window duration in seconds |
| `QueueLimit` | `0` | Queue limit (0 = no queuing, reject immediately) |
| `AddRateLimitHeaders` | `true` | Add rate limit headers to responses |
| `ClientIdHeader` | `X-Client-Id` | Header name for client identification |

## Notes

- Rate limiting requires .NET 7 or later
- On .NET 6, rate limiting is gracefully disabled with a warning
- When `QueueLimit > 0`, excess requests are queued instead of rejected immediately
- Set `QueueLimit = 0` for immediate rejection (recommended for APIs)
- When rate limit is exceeded, returns HTTP 429 Too Many Requests with a JSON ProblemDetails body
