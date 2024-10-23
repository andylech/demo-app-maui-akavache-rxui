using MauiCacheDemo.Api.Shared.Enums;
using MauiCacheDemo.Api.Shared.Models;
using Refit;

namespace MauiCacheDemo.Api;

[Headers("Accept: application/json")]
public interface IPetStoreApi
{
    #region Pet

    // TODO Pass in value

    [Get("/pet/findByStatus?status={status}")]
    Task<IApiResponse<IList<Pet>>> GetPetsByStatus(PetStatus status);

    #endregion
}
