using FluentAssertions;
using Xavier.Internal;
using Xunit;

namespace Xavier.Tests;

public class FeatureDetectionTests
{
    [Fact]
    public void CurrentTfm_ShouldReturnValidTfm()
    {
        // Act
        string tfm = FeatureDetection.CurrentTfm;

        // Assert
        tfm.Should().StartWith("net");
        tfm.Should().MatchRegex(@"net\d+\.\d+");
    }

    [Fact]
    public void IsRateLimitingSupported_ShouldMatchTfm()
    {
        // Act
        bool supported = FeatureDetection.IsRateLimitingSupported;

        // Assert - net7+ should support rate limiting
#if NET7_0_OR_GREATER
        supported.Should().BeTrue();
#else
        supported.Should().BeFalse();
#endif
    }

    [Fact]
    public void IsOpenApiSupported_ShouldMatchTfm()
    {
        // Act
        bool supported = FeatureDetection.IsOpenApiSupported;

        // Assert - net8+ should support built-in OpenAPI
#if NET8_0_OR_GREATER
        supported.Should().BeTrue();
#else
        supported.Should().BeFalse();
#endif
    }

    [Fact]
    public void IsResilienceSupported_ShouldMatchTfm()
    {
        // Act
        bool supported = FeatureDetection.IsResilienceSupported;

        // Assert - net8+ should support standard resilience
#if NET8_0_OR_GREATER
        supported.Should().BeTrue();
#else
        supported.Should().BeFalse();
#endif
    }
}
