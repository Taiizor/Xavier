using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Xavier.Internal;

/// <summary>
/// Helper methods for ProblemDetails customization.
/// </summary>
internal static class ProblemDetailsHelpers
{
    /// <summary>
    /// Adds trace and correlation IDs to ProblemDetails extensions.
    /// </summary>
    public static void EnrichProblemDetails(
        HttpContext context,
        IDictionary<string, object?> extensions,
        bool includeTraceId,
        bool includeCorrelationId)
    {
        if (includeTraceId)
        {
            string traceId = Activity.Current?.Id ?? context.TraceIdentifier;
            extensions["traceId"] = traceId;
        }

        if (includeCorrelationId && context.Items.TryGetValue("CorrelationId", out object? correlationId))
        {
            extensions["correlationId"] = correlationId?.ToString();
        }
    }

    /// <summary>
    /// Gets an appropriate error message for the given exception.
    /// </summary>
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Parameter needed for future use")]
    public static string GetErrorMessage(Exception exception, bool includeDetails)
    {
        if (includeDetails)
        {
            return exception.Message;
        }

        return "An error occurred while processing your request.";
    }
}
