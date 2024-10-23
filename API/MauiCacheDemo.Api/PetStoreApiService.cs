using MauiCacheDemo.Api.Shared.Enums;
using MauiCacheDemo.Api.Shared.Models;
using MauiCacheDemo.Shared;
using MauiCacheDemo.Shared.Enums;
using MauiCacheDemo.Shared.Helpers;
using Refit;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using XamarinFiles.FancyLogger;
using XamarinFiles.PdHelpers.Refit.Models;
using static MauiCacheDemo.Shared.Constants;
using static XamarinFiles.PdHelpers.Refit.Enums.ProblemLevel;
using static XamarinFiles.PdHelpers.Refit.Extractors;

#pragma warning disable CS0162 // Unreachable code detected

namespace MauiCacheDemo.Api;

// TODO Fix alignment issues in FancyLogger
[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
[SuppressMessage("ReSharper", "RedundantArgumentDefaultValue")]
[SuppressMessage("ReSharper", "HeuristicUnreachableCode")]
public class PetStoreApiService : IPetStoreApiService
{
    #region Fields - Static Logger

    private const string AssemblyName = "Petstore API Client";

    private const string ComponentName = "API Service";

    #endregion

    #region Fields - API

    private readonly IPetStoreApi _api;

    #endregion

    #region Fields - Testing

    // True = avoid duplicate logging in API, data service, and/or client
    private const bool DoNotLogResult = true;

    #endregion

    #region Properties

    public TargetEnvironment ApiEnvironment { get; }

    public string ApiEnvironmentName => ApiEnvironment.ToName();

    public string BaseUrl { get; }

    public TargetPlatform ClientPlatform { get; }

    public string ClientPlatformName => ClientPlatform.ToString();

    public string DataSource => BaseUrl;

    private FancyLogger FancyLogger { get; }

    #endregion

    #region Constructors

    public PetStoreApiService(string baseUrl,
        TargetPlatform platform,
        TargetEnvironment environment,
        string componentName = ComponentName)
    {
        try
        {
            FancyLogger = new FancyLogger(allLinesPrefix: componentName);

            ApiCallWrapper.Initialize();

            // TODO Add HTTP logger

            ClientPlatform = platform;
            FancyLogger.LogScalar("Client Platform", ClientPlatformName,
                addIndent: true, newLineAfter: false);

            ApiEnvironment = environment;
            FancyLogger.LogScalar("API Environment", ApiEnvironmentName,
                addIndent: true, newLineAfter: false);

            // Clean up extra characters at the end of the URL in the config
            BaseUrl = baseUrl.TrimEnd('/', ' ');
            FancyLogger.LogScalar("Base URL\t", BaseUrl, addIndent: true,
                newLineAfter: true);

            _api = RestService.For<IPetStoreApi>(BaseUrl, RefitServiceSettings);
        }
        catch (Exception exception)
        {
            SaveExceptionLocation(exception);

            throw;
        }
    }

    #endregion

    #region GET /pet/findByStatus

    public async Task<(List<Pet>?, ProblemReport?)>
        GetPetsByStatus(PetStatus petStatus)
    {
        FancyLogger.LogDebug($"GetPetsByStatus - Request - Status = {petStatus}",
            true, true);

        var response =
            await ApiCallWrapper.CallApiEndpoint(
                async () => await _api.GetPetsByStatus(petStatus));


        if (response is { IsSuccessStatusCode: true, Content: not null })
        {
            var petsByStatus =
                response.Content?.ToList() ?? (List<Pet>) [];

            var msg =
                $"\tGetPetsByStatus - Response - Count = {petsByStatus?.Count ?? 0}";

            if (DoNotLogResult)
            {
                FancyLogger.LogDebug(msg, newLineAfter: true);
            }
            else
            {
                // TODO Switch toggle to show label, label and data, or nothing
                // TODO Switch to LogList when update FancyLogger
                FancyLogger.LogObject<IList<Pet>>(petsByStatus, label: msg);
            }


            return (petsByStatus, null);
        }

        var apiException = response.Error;

        if (apiException is not null)
            SaveExceptionLocation(apiException);

        var problemReport =
            ExtractProblemReport(apiException,
                Error,
                assemblyName: AssemblyName,
                componentName: ComponentName,
                operationName: "GET Pets by Status",
                controllerName: "Pet");

        FancyLogger.LogProblemReport(problemReport);

        return (null, problemReport);
    }

    #endregion

    #region Private

    private static void SaveExceptionLocation(Exception exception,
        [CallerMemberName] string memberName = "",
        [CallerFilePath] string sourceFilePath = "",
        [CallerLineNumber] int sourceLineNumber = 0)
    {
        if (exception.Data.Contains("Member Name") ||
            exception.Data.Contains("Source File Path") ||
            exception.Data.Contains("Source Line Number"))
            return;

        exception.Data.Add("Member Name", memberName);
        exception.Data.Add("Source File Path", sourceFilePath);
        exception.Data.Add("Source Line Number", sourceLineNumber);
    }

    #endregion
}
