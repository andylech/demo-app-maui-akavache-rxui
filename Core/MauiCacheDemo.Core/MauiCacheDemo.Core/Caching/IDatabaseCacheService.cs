using MauiCacheDemo.Core.Enums;
using System;
using System.Reactive;
using System.Threading.Tasks;
using XamarinFiles.PdHelpers.Refit.Models;

namespace MauiCacheDemo.Core.Caching
{
    public interface IDatabaseCacheService
    {
        #region Properties

        string CacheName { get; }

        #endregion

        #region Get Operations

        Task<(T?, ProblemReport?)> GetOrFetchObject<T>(DatabaseCacheKey keyEnum,
            Func<Task<(T?, ProblemReport?)>> fetchFunc,
            bool? forceLatest = false, string? keyId = null);

        Task<T?> GetObject<T>(DatabaseCacheKey keyEnum, string? keyId = null);

        #endregion

        #region Insert Operations

        Task<Unit> InsertObject<T>(DatabaseCacheKey keyEnum, T value,
            string? keyId = null);

        #endregion

        #region Update Operations

        Task<Unit> UpdateObject<T>(DatabaseCacheKey keyEnum, T value,
            string? keyId = null);

        #endregion

        #region Delete Operations

        Task<Unit> InvalidateObject<T>(DatabaseCacheKey keyEnum,
            string? keyId = null);

        IObservable<Unit> InvalidateAll(
            DatabaseCacheLocation? cacheLocation = null);

        #endregion

        #region Maintenance Operations

        void Shutdown();

        #endregion
    }
}
