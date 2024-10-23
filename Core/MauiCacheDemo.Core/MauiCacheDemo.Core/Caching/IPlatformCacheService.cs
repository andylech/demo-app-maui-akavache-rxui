using MauiCacheDemo.Core.Enums;

namespace MauiCacheDemo.Core.Caching;

// Define here for reuse but implement in XF, DNM for platform libraries
public interface IPlatformCacheService
{
    #region Properties

    string CacheName { get; }

    #endregion

    #region Get Operations

    Task<string> GetString(PlatformCacheKey keyEnum);

    Task<T> GetValue<T>(PlatformCacheKey keyEnum);

    #endregion

    #region Remove Operations

    bool DeleteValue(PlatformCacheKey keyEnum);

    bool DeleteAll();

    #endregion

    #region Set Operations

    Task<bool> SaveString(PlatformCacheKey keyEnum, string valueStr);

    Task<bool> SaveValue<T>(PlatformCacheKey keyEnum, T value);

    #endregion
}
