using Xavier;
using Xavier.Extensions;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add Xavier with rate limiting enabled
builder.AddXavier(options =>
{
    // Enable rate limiting
    options.RateLimiting.Enabled = true;

    // Configure rate limiting policies
    options.RateLimiting.EnableGlobalPolicy = true;   // Global rate limit for all requests
    options.RateLimiting.EnablePerIpPolicy = true;    // Per-IP rate limiting
    options.RateLimiting.EnablePerClientPolicy = true; // Per-client rate limiting (using X-Client-Id header)

    // Configure limits
    options.RateLimiting.PermitLimit = 10;     // Allow 10 requests
    options.RateLimiting.WindowSeconds = 60;   // Per 60 seconds window
    options.RateLimiting.QueueLimit = 0;       // No queuing - reject immediately when limit exceeded
    options.RateLimiting.ClientIdHeader = "X-Client-Id"; // Custom header for client identification

    // Enable rate limit headers (default: true)
    // Headers: X-RateLimit-Limit, X-RateLimit-Remaining, X-RateLimit-Reset, Retry-After
    options.RateLimiting.AddRateLimitHeaders = true;
});

WebApplication app = builder.Build();

// Use Xavier middleware (includes rate limiting)
app.UseXavier();

// Map infrastructure endpoints
app.MapXavierInfrastructureEndpoints();

// Public endpoint - no rate limiting
app.MapGet("/", () => "Hello! Try hitting /api/data multiple times to see rate limiting in action. Check response headers for rate limit info!");

// Rate limited with global policy
app.MapGet("/api/data", () => new { Message = "This endpoint uses the global rate limit policy.", Timestamp = DateTime.UtcNow })
    .RequireRateLimiting(XavierDefaults.RateLimiting.GlobalPolicyName);

// Rate limited per IP
app.MapGet("/api/per-ip", () => new { Message = "This endpoint is rate limited per IP address.", Timestamp = DateTime.UtcNow })
    .RequireRateLimiting(XavierDefaults.RateLimiting.PerIpPolicyName);

// Rate limited per client (uses X-Client-Id header)
app.MapGet("/api/per-client", () => new { Message = "This endpoint is rate limited per client (X-Client-Id header).", Timestamp = DateTime.UtcNow })
    .RequireRateLimiting(XavierDefaults.RateLimiting.PerClientPolicyName);

app.Run();
