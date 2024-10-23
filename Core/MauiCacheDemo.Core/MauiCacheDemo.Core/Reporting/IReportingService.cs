using XamarinFiles.PdHelpers.Refit.Models;

namespace MauiCacheDemo.Core.Reporting;

// TODO Add Analytics
public interface IReportingService
{
    #region Standard Reporting Levels w/ Analytics

    void ReportCrash(string messageFormat, params object[] messageArgs);

    void ReportError(string messageFormat, params object[] messageArgs);

    #endregion

    #region Extended Reporting Levels w/ Analytics

    // TODO Add OperationName, PageName, etc to all calls
    void ReportException(Exception exception,
        string sourceOperation = null,
        string resourceName = null);

    void ReportProblem(ProblemReport problemReport);

    #endregion

    #region Standard Reporting Levels w/o Analytics

    void LogDebug(string messageFormat, params object[] messageArgs);

    void LogInfo(string messageFormat, params object[] messageArgs);

    void LogWarning(string messageFormat, params object[] messageArgs);

    #endregion

    #region Extended Reporting Levels w/o Analytics

    void LogCache(string messageFormat, string label = null);

    void LogCacheObject<T>(object obj, bool keepNulls = false,
        string label = null);

    void LogDataLoad(string messageFormat, params object[] messageArgs);

    void LogEmbeddedResources(string[] resourceNames,
        string assemblyName = null);

    void LogNavigation(string messageFormat, params object[] messageArgs);

    void LogObject<T>(object obj, bool keepNulls = false,
        string label = null);

    void LogProblemReport(ProblemReport problemReport);

    void LogValue(string label, object value, bool showNull = false);

    #endregion
}
