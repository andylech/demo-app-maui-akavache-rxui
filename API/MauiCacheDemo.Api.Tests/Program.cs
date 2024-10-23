using MauiCacheDemo.Api.Shared.Enums;
using MauiCacheDemo.Api.Shared.Models;
using MauiCacheDemo.Shared.Enums;
using System.Diagnostics.CodeAnalysis;
using XamarinFiles.FancyLogger;
using XamarinFiles.PdHelpers.Refit.Models;

namespace MauiCacheDemo.Api.Tests;

[SuppressMessage("ReSharper", "RedundantArgumentDefaultValue")]
[SuppressMessage("ReSharper", "UnusedVariable")]
public static class Program
{
    #region Fields - Static Logger

    private const string ApiName = "Petstore";

    private static readonly FancyLogger FancyLogger;

    // HACK Fix prefix logic in FancyLogger
    private const string LoggerPrefix = "API Tester ";

    #endregion

    #region Fields - Configuration

    private const TargetEnvironment Environment = TargetEnvironment.Prod;

    private const TargetPlatform Platform = TargetPlatform.Console;

    private const string Url = "https://petstore.swagger.io/v2/";

    #endregion

    #region Properties

    private static PetStoreApiService? Api { get; }

    #endregion

    #region Constructors

    static Program()
    {
        FancyLogger = new FancyLogger(allLinesPrefix: LoggerPrefix);

        try
        {
            FancyLogger.LogSection(
                $"Testing {ApiName} - {Environment} - {Platform}");

            Api = LoadApiService();

            if (Api is null)
            {
                FancyLogger.LogError(
                    "{ApiName} API Service is Misconfigured",
                    addIndent: false);
            }
        }
        catch (Exception exception)
        {
            FancyLogger.LogException(exception);
        }
    }

    #endregion

    #region Launch

    private static async Task Main()
    {
        if (Api is null)
            return;

        try
        {
            await TestGetPetByStatus(PetStatus.available);
            await TestGetPetByStatus(PetStatus.pending);
            await TestGetPetByStatus(PetStatus.sold);
        }
        catch (Exception exception)
        {
            FancyLogger.LogException(exception);
        }
    }

    #endregion

    #region Private

    private static PetStoreApiService? LoadApiService()
    {
        try
        {
            FancyLogger.LogSubsection($"Mapping {ApiName} Endpoints");

            // TODO Switch to smart constructor
            var apiService = new PetStoreApiService(Url, Platform,
                Environment);

            return apiService;
        }
        catch (Exception exception)
        {
            FancyLogger.LogException(exception);

            return null;
        }
    }

    private static async Task<bool> TestGetPetByStatus(PetStatus petStatus)
    {
        try
        {
            FancyLogger.LogSubsection("Test Finding Pets By Status");

            (List<Pet>? petsByStatus, ProblemReport? problemReport) =
                await Api!.GetPetsByStatus(petStatus);

            return problemReport is null;
        }
        catch (Exception exception)
        {
            FancyLogger.LogException(exception);

            return false;
        }
    }

    #endregion
}
