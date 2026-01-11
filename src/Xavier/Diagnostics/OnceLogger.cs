using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace Xavier.Diagnostics;

/// <summary>
/// Helper class for logging warnings only once per process lifetime.
/// Used to avoid per-request log spam when features are unsupported.
/// </summary>
internal static class OnceLogger
{
    private static readonly ConcurrentDictionary<string, bool> LoggedWarnings = new();

    /// <summary>
    /// Logs a warning message only once per unique key during the process lifetime.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="key">Unique key to identify the warning (typically feature name + reason).</param>
    /// <param name="message">The warning message to log.</param>
    /// <param name="args">Arguments for the message template.</param>
    public static void LogWarningOnce(this ILogger logger, string key, string message, params object?[] args)
    {
        if (logger == null || string.IsNullOrEmpty(key))
        {
            return;
        }

        if (LoggedWarnings.TryAdd(key, true))
        {
            logger.LogWarning(message, args);
        }
    }

    /// <summary>
    /// Logs a warning about an unsupported feature only once per process lifetime.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="featureName">The name of the unsupported feature.</param>
    /// <param name="requiredTfm">The TFM required for the feature.</param>
    /// <param name="currentTfm">The current TFM.</param>
    public static void LogUnsupportedFeatureOnce(
        this ILogger logger,
        string featureName,
        string requiredTfm,
        string currentTfm)
    {
        string key = $"UnsupportedFeature:{featureName}";
        LogWarningOnce(
            logger,
            key,
            "{Feature} requires {RequiredTfm} or later. Feature disabled on {CurrentTfm}.",
            featureName,
            requiredTfm,
            currentTfm);
    }

    /// <summary>
    /// Resets the logged warnings. Primarily for testing purposes.
    /// </summary>
    internal static void Reset()
    {
        LoggedWarnings.Clear();
    }
}
