using Splat;
using System.Collections;
using System.Diagnostics.CodeAnalysis;
using XamarinFiles.FancyLogger;
using XamarinFiles.PdHelpers.Refit.Models;
using static MauiCacheDemo.Shared.Constants;
using static System.Environment;
using static System.String;
using static XamarinFiles.PdHelpers.Refit.Enums.ProblemLevel;
using static XamarinFiles.PdHelpers.Refit.Extractors;

namespace MauiCacheDemo.Core.Reporting;

// TODO Add conditional compilation
// TODO Add crash and exception reporting: App Center? Sentry? RayGun? other?
public class ReportingService : IReportingService
{
    #region Fields - Static Logger

    private static readonly FancyLogger FancyLogger;

    // HACK Fix prefix logic in FancyLogger
    private const string LoggerPrefix = "Reporting Service";

    #endregion

    static ReportingService()
    {
        FancyLogger = Locator.Current.GetService<FancyLogger>()
                      ?? new FancyLogger(allLinesPrefix: LoggerPrefix);
    }

    #region Standard Reporting Levels w/ Analytics

    public void ReportCrash(string messageFormat, params object[] messageArgs)
    {
        // TODO Create LogCrash in FancyLogger after port over from old
        FancyLogger.LogError(messageFormat, args: messageArgs);

        // TODO Send crash to analytics service
    }

    public void ReportError(string messageFormat, params object[] messageArgs)
    {
        var message = FormatMessage(messageFormat, messageArgs);

        FancyLogger.LogError(message);

        // TODO
        // Analytics.TrackEvent(message);
    }

    #endregion

    #region Extended Reporting Methods w/ Analytics

    // TODO Add bool showStackTrace = false after add from old FL library
    [SuppressMessage("ReSharper", "UnusedVariable")]
    [SuppressMessage("Style", "IDE0059:Unnecessary assignment of a value", Justification = "<Pending>")]
    public void
        ReportException(Exception exception,
            string? sourceOperation = null,
            string? resourceName = null)
    {
        FancyLogger.LogException(exception);

        var problemReport =
            ExtractProblemReport(exception, Error,
                sourceOperation, resourceName);

        (_, Dictionary<string, string>? eventContext) =
            ExtractEventContext(problemReport);

        // TODO
        //Crashes.TrackError(exception, eventContext);
    }

    [SuppressMessage("ReSharper", "UnusedVariable")]
    [SuppressMessage("Style", "IDE0059:Unnecessary assignment of a value", Justification = "<Pending>")]
    public void
        ReportProblem(ProblemReport? problemReport)
    {
        if (problemReport == null)
            return;

        FancyLogger.LogProblemReport(problemReport);

        (string? eventName, Dictionary<string, string>? eventContext) =
            ExtractEventContext(problemReport);

        // TODO
        //Analytics.TrackEvent(eventName, eventContext);
    }

    #endregion

    #region Standard Reporting Levels w/o Analytics

    public void LogDebug(string format, params object[] args)
    {
        FancyLogger.LogDebug(format, args: args);
    }

    public void LogInfo(string format, params object[] args)
    {
        FancyLogger.LogInfo(format, args: args);
    }

    public void LogWarning(string format, params object[] args)
    {
        FancyLogger.LogWarning(format, args: args);
    }

    #endregion

    #region Extended Reporting Methods w/o Analytics

    public void LogCache(string str, string? label = null)
    {
        var messageFormat = "CACHE: ";
        if (!IsNullOrWhiteSpace(label))
            messageFormat += label + " = ";
        messageFormat += "{0}";

        LogDebug(messageFormat, str);
    }

    // TODO Distinguish bt. database vs platform cache and read vs write ops
    public void LogCacheObject<T>(object? obj, bool keepNulls = false,
        string? label = null)
    {
        if (obj is null)
            return;

        var cacheLabel = "CACHE: ";
        cacheLabel += !IsNullOrWhiteSpace(label) ? label : GetType().Name;

        if (obj is IEnumerable enumerable)
        {
            var count = enumerable.Cast<object>().Count();

            cacheLabel += $" ({count})";
        }

        LogObject<T>(obj, keepNulls, cacheLabel);
    }

    public void LogDataLoad(string messageFormat, params object[] messageArgs)
    {
        var navMessageFormat = "DATA LOAD: " + messageFormat;

        LogDebug(navMessageFormat, messageArgs);
    }

    // TODO Combine with ConfigurationService.PrintEmbeddedResources
    // TODO Update after add list printer back to FancyLogger
    public void LogEmbeddedResources(string[] resourceNames,
        string assemblyName)
    {
        var message = $"{assemblyName} - Embedded Resources";

        message = resourceNames.Aggregate(message,
            (current, resourceName) =>
                current + NewLine + Indent + resourceName);

        // Workaround for nullable difference with FancyLogger .NET version
        message += NewLine;

        LogInfo(message);
    }

    public void LogNavigation(string messageFormat, params object[] messageArgs)
    {
        var navMessageFormat = "NAVIGATION: " + messageFormat;

        LogInfo(navMessageFormat, messageArgs);
    }

    public void LogObject<T>(object? obj, bool keepNulls = false,
        string? label = null)
    {
        FancyLogger.LogObject<T>(obj, keepNulls: keepNulls, label:label);
    }

    public void LogProblemReport(ProblemReport problemReport)
    {
        FancyLogger.LogProblemReport(problemReport);
    }

    public void LogValue(string label, object? value, bool showNull = false)
    {
        if (!showNull && value is null)
            return;

        FancyLogger.LogScalar(label, value?.ToString());
    }

    #endregion

    #region Private

    private static string FormatMessage(string messageFormat, object[] messageArgs)
    {
        if (messageArgs.Length < 1)
            return messageFormat;

        var message = Format(messageFormat, messageArgs);

        return message;
    }

    #endregion
}
