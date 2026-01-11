using FluentAssertions;
using Microsoft.AspNetCore.TestHost;
using System.Net;
using Xavier.Extensions;
using Xunit;

namespace Xavier.IntegrationTests;

public class HealthEndpointTests
{
    [Fact]
    public async Task Health_ShouldReturn200()
    {
        // Arrange
        using IHost host = await CreateTestHost();
        HttpClient client = host.GetTestClient();

        // Act
        HttpResponseMessage response = await client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Ready_ShouldReturn200()
    {
        // Arrange
        using IHost host = await CreateTestHost();
        HttpClient client = host.GetTestClient();

        // Act
        HttpResponseMessage response = await client.GetAsync("/ready");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Health_WithCustomPath_ShouldReturn200()
    {
        // Arrange
        using IHost host = await CreateTestHost(options =>
        {
            options.Health.LivenessPath = "/healthz";
            options.Health.ReadinessPath = "/readyz";
        });
        HttpClient client = host.GetTestClient();

        // Act
        HttpResponseMessage response = await client.GetAsync("/healthz");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Health_WithIncludeDetails_ShouldReturnJson()
    {
        // Arrange
        using IHost host = await CreateTestHost(options =>
        {
            options.Health.IncludeDetails = true;
        });
        HttpClient client = host.GetTestClient();

        // Act
        HttpResponseMessage response = await client.GetAsync("/health");
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("status");
    }

    private static async Task<IHost> CreateTestHost(Action<Xavier.Options.XavierOptions>? configure = null)
    {
        return await new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder
                    .UseTestServer()
                    .ConfigureServices((context, services) =>
                    {
                        services.AddRouting();
                        services.AddXavier(context.Configuration, configure);
                    })
                    .Configure(app =>
                    {
                        app.UseRouting();
                        app.UseXavier();
                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapXavierInfrastructureEndpoints();
                        });
                    });
            })
            .StartAsync();
    }
}

public class CorrelationIdTests
{
    [Fact]
    public async Task Request_ShouldReturnCorrelationIdHeader()
    {
        // Arrange
        using IHost host = await CreateTestHost();
        HttpClient client = host.GetTestClient();

        // Act
        HttpResponseMessage response = await client.GetAsync("/health");

        // Assert
        response.Headers.Should().ContainKey("X-Correlation-Id");
        response.Headers.GetValues("X-Correlation-Id").First().Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Request_WithCorrelationId_ShouldReturnSameId()
    {
        // Arrange
        using IHost host = await CreateTestHost();
        HttpClient client = host.GetTestClient();
        string correlationId = Guid.NewGuid().ToString("N");
        HttpRequestMessage request = new(HttpMethod.Get, "/health");
        request.Headers.Add("X-Correlation-Id", correlationId);

        // Act
        HttpResponseMessage response = await client.SendAsync(request);

        // Assert
        response.Headers.GetValues("X-Correlation-Id").First().Should().Be(correlationId);
    }

    [Fact]
    public async Task Request_WithCustomHeader_ShouldUseCustomHeader()
    {
        // Arrange
        using IHost host = await CreateTestHost(options =>
        {
            options.Correlation.HeaderName = "X-Request-Id";
        });
        HttpClient client = host.GetTestClient();
        string correlationId = Guid.NewGuid().ToString("N");
        HttpRequestMessage request = new(HttpMethod.Get, "/health");
        request.Headers.Add("X-Request-Id", correlationId);

        // Act
        HttpResponseMessage response = await client.SendAsync(request);

        // Assert - HTTP header names are case-insensitive per RFC 7230
        string headerKey = response.Headers.FirstOrDefault(h =>
            h.Key.Equals("X-Request-Id", StringComparison.OrdinalIgnoreCase)).Key;
        headerKey.Should().NotBeNullOrEmpty("Response should contain X-Request-Id header");
        response.Headers.GetValues(headerKey).First().Should().Be(correlationId);
    }

    [Fact]
    public async Task Request_WithDisabledCorrelation_ShouldNotReturnHeader()
    {
        // Arrange
        using IHost host = await CreateTestHost(options =>
        {
            options.Correlation.Enabled = false;
        });
        HttpClient client = host.GetTestClient();

        // Act
        HttpResponseMessage response = await client.GetAsync("/health");

        // Assert
        response.Headers.Should().NotContainKey("X-Correlation-Id");
    }

    [Fact]
    public async Task Request_WithAddToResponseDisabled_ShouldNotReturnHeader()
    {
        // Arrange
        using IHost host = await CreateTestHost(options =>
        {
            options.Correlation.AddToResponse = false;
        });
        HttpClient client = host.GetTestClient();

        // Act
        HttpResponseMessage response = await client.GetAsync("/health");

        // Assert
        response.Headers.Should().NotContainKey("X-Correlation-Id");
    }

    private static async Task<IHost> CreateTestHost(Action<Xavier.Options.XavierOptions>? configure = null)
    {
        return await new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder
                    .UseTestServer()
                    .ConfigureServices((context, services) =>
                    {
                        services.AddRouting();
                        services.AddXavier(context.Configuration, configure);
                    })
                    .Configure(app =>
                    {
                        app.UseRouting();
                        app.UseXavier();
                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapXavierInfrastructureEndpoints();
                        });
                    });
            })
            .StartAsync();
    }
}

public class ErrorHandlingTests
{
    [Fact]
    public async Task Exception_ShouldReturnProblemDetails()
    {
        // Arrange
        using IHost host = await CreateTestHost();
        HttpClient client = host.GetTestClient();

        // Act
        HttpResponseMessage response = await client.GetAsync("/api/error");
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        response.Content.Headers.ContentType?.MediaType.Should().Contain("problem+json");
    }

    [Fact]
    public async Task NotFound_ShouldReturnProblemDetails()
    {
        // Arrange
        using IHost host = await CreateTestHost();
        HttpClient client = host.GetTestClient();

        // Act
        HttpResponseMessage response = await client.GetAsync("/api/nonexistent");
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Exception_ShouldIncludeCorrelationId()
    {
        // Arrange
        using IHost host = await CreateTestHost();
        HttpClient client = host.GetTestClient();
        string correlationId = Guid.NewGuid().ToString("N");
        HttpRequestMessage request = new(HttpMethod.Get, "/api/error");
        request.Headers.Add("X-Correlation-Id", correlationId);

        // Act
        HttpResponseMessage response = await client.SendAsync(request);

        // Assert
        response.Headers.Should().ContainKey("X-Correlation-Id");
        response.Headers.GetValues("X-Correlation-Id").First().Should().Be(correlationId);
    }

    [Fact]
    public async Task Exception_WithIncludeExceptionDetails_ShouldIncludeExceptionInfo()
    {
        // Arrange
        using IHost host = await CreateTestHost(options =>
        {
            options.ErrorHandling.IncludeExceptionDetails = true;
        });
        HttpClient client = host.GetTestClient();

        // Act
        HttpResponseMessage response = await client.GetAsync("/api/error");
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        content.Should().Contain("Test exception"); // Exception message should be included
        content.Should().Contain("detail"); // Detail field should be present
    }

    [Fact]
    public async Task Exception_WithoutIncludeExceptionDetails_ShouldNotIncludeExceptionInfo()
    {
        // Arrange
        using IHost host = await CreateTestHost(options =>
        {
            options.ErrorHandling.IncludeExceptionDetails = false;
        }, isDevelopment: false);
        HttpClient client = host.GetTestClient();

        // Act
        HttpResponseMessage response = await client.GetAsync("/api/error");
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        content.Should().NotContain("Test exception"); // Exception message should NOT be included
        content.Should().NotContain("stackTrace"); // Stack trace should NOT be included
    }

    [Fact]
    public async Task Exception_WithIncludeTraceId_ShouldIncludeTraceId()
    {
        // Arrange
        using IHost host = await CreateTestHost(options =>
        {
            options.ErrorHandling.IncludeTraceId = true;
        });
        HttpClient client = host.GetTestClient();

        // Act
        HttpResponseMessage response = await client.GetAsync("/api/error");
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        content.Should().Contain("traceId"); // Trace ID should be present
    }

    [Fact]
    public async Task Exception_WithoutIncludeTraceId_ShouldNotIncludeTraceId()
    {
        // Arrange
        using IHost host = await CreateTestHost(options =>
        {
            options.ErrorHandling.IncludeTraceId = false;
        });
        HttpClient client = host.GetTestClient();

        // Act
        HttpResponseMessage response = await client.GetAsync("/api/error");
        string content = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        content.Should().NotContain("traceId"); // Trace ID should NOT be present
    }

    private static async Task<IHost> CreateTestHost(Action<Xavier.Options.XavierOptions>? configure = null, bool isDevelopment = true)
    {
        return await new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder
                    .UseTestServer()
                    .UseEnvironment(isDevelopment ? "Development" : "Production")
                    .ConfigureServices((context, services) =>
                    {
                        services.AddRouting();
                        services.AddXavier(context.Configuration, configure);
                    })
                    .Configure(app =>
                    {
                        app.UseRouting();
                        app.UseXavier();
                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapXavierInfrastructureEndpoints();
                            endpoints.MapGet("/api/error", () =>
                            {
                                throw new InvalidOperationException("Test exception");
                            });
                        });
                    });
            })
            .StartAsync();
    }
}

public class RateLimitingTests
{
#if NET7_0_OR_GREATER
    [Fact]
    public async Task RateLimiting_WhenEnabled_ShouldLimitRequests()
    {
        // Arrange
        using IHost host = await CreateTestHost(options =>
        {
            options.RateLimiting.Enabled = true;
            options.RateLimiting.PermitLimit = 2;
            options.RateLimiting.WindowSeconds = 60;
        });
        HttpClient client = host.GetTestClient();

        // Act - Send more requests than the limit
        List<HttpResponseMessage> responses = [];
        for (int i = 0; i < 5; i++)
        {
            responses.Add(await client.GetAsync("/api/test"));
        }

        // Assert - Some requests should be rate limited
        responses.Should().Contain(r => r.StatusCode == HttpStatusCode.TooManyRequests);
    }

    [Fact]
    public async Task RateLimiting_SuccessfulRequest_ShouldReturnRateLimitHeaders()
    {
        // Arrange
        using IHost host = await CreateTestHost(options =>
        {
            options.RateLimiting.Enabled = true;
            options.RateLimiting.PermitLimit = 10;
            options.RateLimiting.WindowSeconds = 60;
            options.RateLimiting.AddRateLimitHeaders = true;
        });
        HttpClient client = host.GetTestClient();

        // Act - First request should succeed with headers
        HttpResponseMessage response = await client.GetAsync("/api/test");

        // Assert - Response should have rate limit headers even on success
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.Should().ContainKey("X-RateLimit-Limit");
        response.Headers.Should().ContainKey("X-RateLimit-Remaining");
        response.Headers.Should().ContainKey("X-RateLimit-Reset");
        response.Headers.Should().ContainKey("X-RateLimit-Used");

        // Verify header values
        response.Headers.GetValues("X-RateLimit-Limit").First().Should().Be("10");
        response.Headers.GetValues("X-RateLimit-Used").First().Should().Be("1");
        response.Headers.GetValues("X-RateLimit-Remaining").First().Should().Be("9");
    }

    [Fact]
    public async Task RateLimiting_RemainingCount_ShouldDecreaseWithEachRequest()
    {
        // Arrange
        using IHost host = await CreateTestHost(options =>
        {
            options.RateLimiting.Enabled = true;
            options.RateLimiting.PermitLimit = 5;
            options.RateLimiting.WindowSeconds = 60;
            options.RateLimiting.AddRateLimitHeaders = true;
        });
        HttpClient client = host.GetTestClient();

        // Act - Send 3 requests and check remaining decreases
        HttpResponseMessage response1 = await client.GetAsync("/api/test");
        HttpResponseMessage response2 = await client.GetAsync("/api/test");
        HttpResponseMessage response3 = await client.GetAsync("/api/test");

        // Assert - Remaining should decrease with each request
        response1.Headers.GetValues("X-RateLimit-Remaining").First().Should().Be("4");
        response1.Headers.GetValues("X-RateLimit-Used").First().Should().Be("1");

        response2.Headers.GetValues("X-RateLimit-Remaining").First().Should().Be("3");
        response2.Headers.GetValues("X-RateLimit-Used").First().Should().Be("2");

        response3.Headers.GetValues("X-RateLimit-Remaining").First().Should().Be("2");
        response3.Headers.GetValues("X-RateLimit-Used").First().Should().Be("3");
    }

    [Fact]
    public async Task RateLimiting_WhenLimitExceeded_ShouldReturnRateLimitHeaders()
    {
        // Arrange
        using IHost host = await CreateTestHost(options =>
        {
            options.RateLimiting.Enabled = true;
            options.RateLimiting.PermitLimit = 1;
            options.RateLimiting.WindowSeconds = 60;
            options.RateLimiting.AddRateLimitHeaders = true;
        });
        HttpClient client = host.GetTestClient();

        // Act - First request should succeed, second should be rate limited
        await client.GetAsync("/api/test");
        HttpResponseMessage response = await client.GetAsync("/api/test");

        // Assert - Response should have rate limit headers
        response.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
        response.Headers.Should().ContainKey("X-RateLimit-Limit");
        response.Headers.Should().ContainKey("X-RateLimit-Remaining");
        response.Headers.Should().ContainKey("X-RateLimit-Reset");
        response.Headers.Should().ContainKey("X-RateLimit-Used");
        response.Headers.Should().ContainKey("Retry-After");

        // Verify header values
        response.Headers.GetValues("X-RateLimit-Limit").First().Should().Be("1");
        response.Headers.GetValues("X-RateLimit-Remaining").First().Should().Be("0");
        response.Headers.GetValues("X-RateLimit-Used").First().Should().Be("1");
    }

    [Fact]
    public async Task RateLimiting_ResetTime_ShouldBeConsistent()
    {
        // Arrange
        using IHost host = await CreateTestHost(options =>
        {
            options.RateLimiting.Enabled = true;
            options.RateLimiting.PermitLimit = 1;
            options.RateLimiting.WindowSeconds = 60;
            options.RateLimiting.AddRateLimitHeaders = true;
        });
        HttpClient client = host.GetTestClient();

        // Act - Exceed limit and make multiple 429 requests
        await client.GetAsync("/api/test"); // This should succeed
        HttpResponseMessage response1 = await client.GetAsync("/api/test"); // This should be 429
        HttpResponseMessage response2 = await client.GetAsync("/api/test"); // This should also be 429

        // Assert - Both 429 responses should have same reset time
        response1.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);
        response2.StatusCode.Should().Be(HttpStatusCode.TooManyRequests);

        string resetTime1 = response1.Headers.GetValues("X-RateLimit-Reset").First();
        string resetTime2 = response2.Headers.GetValues("X-RateLimit-Reset").First();

        // Reset times should be identical (same window)
        resetTime1.Should().Be(resetTime2);
    }
#endif

    private static async Task<IHost> CreateTestHost(Action<Xavier.Options.XavierOptions>? configure = null)
    {
        return await new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder
                    .UseTestServer()
                    .ConfigureServices((context, services) =>
                    {
                        services.AddRouting();
                        services.AddXavier(context.Configuration, configure);
                    })
                    .Configure(app =>
                    {
                        app.UseRouting();
                        app.UseXavier();
                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapXavierInfrastructureEndpoints();
                            endpoints.MapGet("/api/test", () => "OK")
#if NET7_0_OR_GREATER
                                .RequireRateLimiting(XavierDefaults.RateLimiting.GlobalPolicyName)
#endif
                                ;
                        });
                    });
            })
            .StartAsync();
    }
}

public class HttpLoggingTests
{
    [Fact]
    public async Task HttpLogging_WhenEnabled_ShouldNotAffectResponse()
    {
        // Arrange
        using IHost host = await CreateTestHost(options =>
        {
            options.HttpLogging.Enabled = true;
        });
        HttpClient client = host.GetTestClient();

        // Act
        HttpResponseMessage response = await client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private static async Task<IHost> CreateTestHost(Action<Xavier.Options.XavierOptions>? configure = null)
    {
        return await new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder
                    .UseTestServer()
                    .ConfigureServices((context, services) =>
                    {
                        services.AddRouting();
                        services.AddXavier(context.Configuration, configure);
                    })
                    .Configure(app =>
                    {
                        app.UseRouting();
                        app.UseXavier();
                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapXavierInfrastructureEndpoints();
                        });
                    });
            })
            .StartAsync();
    }
}

public class ConfigurationBindingTests
{
    [Fact]
    public async Task AppSettings_ShouldBindXavierOptions()
    {
        // Arrange - Use in-memory configuration to simulate appsettings.json
        Dictionary<string, string?> configValues = new()
        {
            ["Xavier:Correlation:Enabled"] = "true",
            ["Xavier:Correlation:HeaderName"] = "X-Custom-Request-Id",
            ["Xavier:Health:Enabled"] = "true",
            ["Xavier:Health:LivenessPath"] = "/custom-health",
            ["Xavier:Health:ReadinessPath"] = "/custom-ready"
        };

        using IHost host = await CreateTestHostWithConfig(configValues);
        HttpClient client = host.GetTestClient();

        // Act - Test custom health path from config
        HttpResponseMessage response = await client.GetAsync("/custom-health");

        // Assert - Config binding should work
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AppSettings_CorrelationHeaderName_ShouldBeApplied()
    {
        // Arrange - Use in-memory configuration to simulate appsettings.json
        Dictionary<string, string?> configValues = new()
        {
            ["Xavier:Correlation:Enabled"] = "true",
            ["Xavier:Correlation:HeaderName"] = "X-Custom-Trace-Id",
            ["Xavier:Health:Enabled"] = "true"
        };

        using IHost host = await CreateTestHostWithConfig(configValues);
        HttpClient client = host.GetTestClient();

        string customCorrelationId = Guid.NewGuid().ToString("N");
        HttpRequestMessage request = new(HttpMethod.Get, "/health");
        request.Headers.Add("X-Custom-Trace-Id", customCorrelationId);

        // Act
        HttpResponseMessage response = await client.SendAsync(request);

        // Assert - The custom header name from config should be used
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        string headerKey = response.Headers.FirstOrDefault(h =>
            h.Key.Equals("X-Custom-Trace-Id", StringComparison.OrdinalIgnoreCase)).Key;
        headerKey.Should().NotBeNullOrEmpty("Response should contain X-Custom-Trace-Id header");
        response.Headers.GetValues(headerKey).First().Should().Be(customCorrelationId);
    }

    [Fact]
    public async Task AppSettings_ProgrammaticConfig_ShouldOverrideAppSettings()
    {
        // Arrange - Use in-memory configuration to simulate appsettings.json
        Dictionary<string, string?> configValues = new()
        {
            ["Xavier:Health:Enabled"] = "true",
            ["Xavier:Health:LivenessPath"] = "/health-from-config"
        };

        // Programmatic configuration should override appsettings.json
        using IHost host = await CreateTestHostWithConfig(configValues, options =>
        {
            options.Health.LivenessPath = "/health-from-code";
        });
        HttpClient client = host.GetTestClient();

        // Act - The programmatic path should take precedence
        HttpResponseMessage responseFromConfig = await client.GetAsync("/health-from-config");
        HttpResponseMessage responseFromCode = await client.GetAsync("/health-from-code");

        // Assert - Code config should override appsettings.json
        responseFromConfig.StatusCode.Should().Be(HttpStatusCode.NotFound);
        responseFromCode.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task AppSettings_DisabledCorrelation_ShouldNotReturnHeader()
    {
        // Arrange - Use in-memory configuration to simulate appsettings.json
        Dictionary<string, string?> configValues = new()
        {
            ["Xavier:Correlation:Enabled"] = "false",
            ["Xavier:Health:Enabled"] = "true"
        };

        using IHost host = await CreateTestHostWithConfig(configValues);
        HttpClient client = host.GetTestClient();

        // Act
        HttpResponseMessage response = await client.GetAsync("/health");

        // Assert - Correlation should be disabled from config
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.Should().NotContainKey("X-Correlation-Id");
    }

    private static async Task<IHost> CreateTestHostWithConfig(
        Dictionary<string, string?> configValues,
        Action<Xavier.Options.XavierOptions>? configure = null)
    {
        return await new HostBuilder()
            .ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(configValues);
            })
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder
                    .UseTestServer()
                    .ConfigureServices((context, services) =>
                    {
                        services.AddRouting();
                        services.AddXavier(context.Configuration, configure);
                    })
                    .Configure(app =>
                    {
                        app.UseRouting();
                        app.UseXavier();
                        app.UseEndpoints(endpoints =>
                        {
                            endpoints.MapXavierInfrastructureEndpoints();
                        });
                    });
            })
            .StartAsync();
    }
}
