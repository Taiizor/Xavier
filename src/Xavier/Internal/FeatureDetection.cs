using System.Runtime.CompilerServices;

namespace Xavier.Internal;

/// <summary>
/// Provides TFM-based feature availability detection.
/// </summary>
internal static class FeatureDetection
{
    /// <summary>
    /// Gets whether rate limiting is supported on the current TFM.
    /// Rate limiting requires .NET 7 or later.
    /// </summary>
    public static bool IsRateLimitingSupported
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get =>
#if NET7_0_OR_GREATER
          true;
#else
          false;
#endif

    }

    /// <summary>
    /// Gets whether built-in OpenAPI is supported on the current TFM.
    /// Built-in OpenAPI requires .NET 8 or later.
    /// </summary>
    public static bool IsOpenApiSupported
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get =>
#if NET8_0_OR_GREATER
          true;
#else
          false;
#endif

    }

    /// <summary>
    /// Gets whether standard HTTP resilience handler is supported on the current TFM.
    /// Standard resilience requires .NET 8 or later.
    /// </summary>
    public static bool IsResilienceSupported
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get =>
#if NET8_0_OR_GREATER
          true;
#else
          false;
#endif

    }

    /// <summary>
    /// Gets the current TFM moniker.
    /// </summary>
    public static string CurrentTfm =>
#if NET10_0_OR_GREATER
            "net10.0";
#elif NET9_0
            "net9.0";
#elif NET8_0
            "net8.0";
#elif NET7_0
            "net7.0";
#else
            "net6.0";
#endif

}
