using MauiCacheDemo.Api.Shared.Enums;
using MauiCacheDemo.Api.Shared.Models;
using MauiCacheDemo.Shared.Enums;
using XamarinFiles.PdHelpers.Refit.Models;

namespace MauiCacheDemo.Api;

public interface IPetStoreApiService
{
    #region Data Source

    TargetEnvironment ApiEnvironment { get; }

    string ApiEnvironmentName { get; }

    string DataSource { get; }

    #endregion

    #region Pet

    Task<(List<Pet>?, ProblemReport?)> GetPetsByStatus(PetStatus petStatus);

    #endregion
}
