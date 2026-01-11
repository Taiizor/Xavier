using FluentAssertions;
using Xavier.Options;
using Xunit;

namespace Xavier.Tests;

public class XavierOptionsTests
{
    [Fact]
    public void XavierOptions_ShouldHaveCorrectDefaults()
    {
        // Arrange & Act
        XavierOptions options = new();

        // Assert
        options.ErrorHandling.Enabled.Should().BeTrue();
        options.Correlation.Enabled.Should().BeTrue();
        options.HttpLogging.Enabled.Should().BeFalse();
        options.RateLimiting.Enabled.Should().BeFalse();
        options.OpenApi.Enabled.Should().BeTrue();
        options.Health.Enabled.Should().BeTrue();
        options.Resilience.Enabled.Should().BeFalse();
        options.Observability.Enabled.Should().BeFalse();
    }

    [Fact]
    public void ErrorHandlingOptions_ShouldHaveCorrectDefaults()
    {
        // Arrange & Act
        ErrorHandlingOptions options = new();

        // Assert
        options.Enabled.Should().BeTrue();
        options.IncludeExceptionDetails.Should().BeFalse();
        options.IncludeTraceId.Should().BeTrue();
        options.IncludeCorrelationId.Should().BeTrue();
    }

    [Fact]
    public void CorrelationOptions_ShouldHaveCorrectDefaults()
    {
        // Arrange & Act
        CorrelationOptions options = new();

        // Assert
        options.Enabled.Should().BeTrue();
        options.HeaderName.Should().Be(XavierDefaults.Correlation.HeaderName);
        options.AddToResponse.Should().BeTrue();
        options.UseTraceparent.Should().BeFalse(); // Default should be false per spec
        options.EnrichLogScopes.Should().BeTrue();
    }

    [Fact]
    public void HttpLoggingOptions_ShouldHaveCorrectDefaults()
    {
        // Arrange & Act
        HttpLoggingOptions options = new();

        // Assert
        options.Enabled.Should().BeFalse();
        options.LogRequestHeaders.Should().BeTrue();
        options.LogResponseHeaders.Should().BeTrue();
        options.LogRequestBody.Should().BeFalse();
        options.LogResponseBody.Should().BeFalse();
        options.ExcludedHeaders.Should().Contain("Authorization");
        options.ExcludedHeaders.Should().Contain("Cookie");
        options.ExcludedHeaders.Should().Contain("Set-Cookie");
        options.ExcludedHeaders.Should().Contain("X-Api-Key");
        options.ExcludedHeaders.Should().Contain("X-Auth-Token");
        options.RequestBodyLogLimit.Should().Be(32 * 1024);
        options.ResponseBodyLogLimit.Should().Be(32 * 1024);
    }

    [Fact]
    public void RateLimitingOptions_ShouldHaveCorrectDefaults()
    {
        // Arrange & Act
        RateLimitingOptions options = new();

        // Assert
        options.Enabled.Should().BeFalse();
        options.EnableGlobalPolicy.Should().BeTrue();
        options.EnablePerIpPolicy.Should().BeTrue();
        options.EnablePerClientPolicy.Should().BeFalse();
        options.ClientIdHeader.Should().Be(XavierDefaults.RateLimiting.ClientIdHeader);
        options.PermitLimit.Should().Be(XavierDefaults.RateLimiting.DefaultPermitLimit);
        options.WindowSeconds.Should().Be(XavierDefaults.RateLimiting.DefaultWindowSeconds);
        options.QueueLimit.Should().Be(0);
        options.AddRateLimitHeaders.Should().BeTrue();
    }

    [Fact]
    public void HealthOptions_ShouldHaveCorrectDefaults()
    {
        // Arrange & Act
        HealthOptions options = new();

        // Assert
        options.Enabled.Should().BeTrue();
        options.LivenessPath.Should().Be(XavierDefaults.Health.LivenessPath);
        options.ReadinessPath.Should().Be(XavierDefaults.Health.ReadinessPath);
        options.ReadinessTags.Should().Contain(XavierDefaults.Health.ReadinessTag);
        options.IncludeDetails.Should().BeFalse();
    }

    [Fact]
    public void ObservabilityOptions_ShouldHaveCorrectDefaults()
    {
        // Arrange & Act
        ObservabilityOptions options = new();

        // Assert
        options.Enabled.Should().BeFalse();
        options.ServiceName.Should().BeNull();
        options.ServiceVersion.Should().BeNull();
        options.EnableTracing.Should().BeTrue();
        options.EnableMetrics.Should().BeTrue();
        options.EnableRuntimeMetrics.Should().BeTrue();
        options.ConfigureTracing.Should().BeNull();
        options.ConfigureMetrics.Should().BeNull();
    }

    [Fact]
    public void OpenApiOptions_ShouldHaveCorrectDefaults()
    {
        // Arrange & Act
        OpenApiOptions options = new();

        // Assert
        options.Enabled.Should().BeTrue();
        options.DocumentName.Should().Be(XavierDefaults.OpenApi.DocumentName);
        options.PathTemplate.Should().Be(XavierDefaults.OpenApi.PathTemplate);
        options.ExposeInNonDevelopment.Should().BeFalse();
        options.Title.Should().BeNull();
        options.Version.Should().BeNull();
    }

    [Fact]
    public void ResilienceOptions_ShouldHaveCorrectDefaults()
    {
        // Arrange & Act
        ResilienceOptions options = new();

        // Assert
        options.Enabled.Should().BeFalse();
        options.UseStandardResilience.Should().BeTrue();
        options.MaxRetryAttempts.Should().Be(3);
        options.TimeoutSeconds.Should().Be(30);
    }

    [Fact]
    public void XavierAppOptions_ShouldHaveCorrectDefaults()
    {
        // Arrange & Act
        XavierAppOptions options = new();

        // Assert
        options.UseCorrelation.Should().BeTrue();
        options.UseErrorHandling.Should().BeTrue();
        options.UseRateLimiting.Should().BeTrue();
        options.UseHttpLogging.Should().BeTrue();
    }

    [Fact]
    public void XavierEndpointOptions_ShouldHaveCorrectDefaults()
    {
        // Arrange & Act
        XavierEndpointOptions options = new();

        // Assert
        options.MapHealthEndpoints.Should().BeTrue();
        options.MapOpenApiEndpoints.Should().BeTrue();
        options.RoutePrefix.Should().BeNull();
        options.EndpointTags.Should().BeEmpty();
    }

    [Fact]
    public void XavierDefaults_ShouldHaveCorrectValues()
    {
        // Assert Configuration Section
        XavierDefaults.ConfigurationSection.Should().Be("Xavier");

        // Assert Correlation defaults
        XavierDefaults.Correlation.HeaderName.Should().Be("X-Correlation-Id");
        XavierDefaults.Correlation.TraceparentHeader.Should().Be("traceparent");

        // Assert Health defaults
        XavierDefaults.Health.LivenessPath.Should().Be("/health");
        XavierDefaults.Health.ReadinessPath.Should().Be("/ready");
        XavierDefaults.Health.ReadinessTag.Should().Be("ready");

        // Assert OpenApi defaults
        XavierDefaults.OpenApi.DocumentName.Should().Be("v1");
        XavierDefaults.OpenApi.PathTemplate.Should().Be("/openapi/{documentName}.json");

        // Assert RateLimiting defaults
        XavierDefaults.RateLimiting.GlobalPolicyName.Should().Be("Xavier.Global");
        XavierDefaults.RateLimiting.PerIpPolicyName.Should().Be("Xavier.PerIP");
        XavierDefaults.RateLimiting.PerClientPolicyName.Should().Be("Xavier.PerClient");
        XavierDefaults.RateLimiting.ClientIdHeader.Should().Be("X-Client-Id");
        XavierDefaults.RateLimiting.DefaultPermitLimit.Should().Be(100);
        XavierDefaults.RateLimiting.DefaultWindowSeconds.Should().Be(60);
        XavierDefaults.RateLimiting.HeaderLimit.Should().Be("X-RateLimit-Limit");
        XavierDefaults.RateLimiting.HeaderRemaining.Should().Be("X-RateLimit-Remaining");
        XavierDefaults.RateLimiting.HeaderReset.Should().Be("X-RateLimit-Reset");
        XavierDefaults.RateLimiting.HeaderUsed.Should().Be("X-RateLimit-Used");
        XavierDefaults.RateLimiting.HeaderRetryAfter.Should().Be("Retry-After");

        // Assert HttpLogging defaults
        XavierDefaults.HttpLogging.ExcludedHeaders.Should().Contain("Authorization");
        XavierDefaults.HttpLogging.ExcludedHeaders.Should().Contain("Cookie");
        XavierDefaults.HttpLogging.ExcludedHeaders.Should().Contain("Set-Cookie");
        XavierDefaults.HttpLogging.ExcludedHeaders.Should().Contain("X-Api-Key");
        XavierDefaults.HttpLogging.ExcludedHeaders.Should().Contain("X-Auth-Token");
    }
}
