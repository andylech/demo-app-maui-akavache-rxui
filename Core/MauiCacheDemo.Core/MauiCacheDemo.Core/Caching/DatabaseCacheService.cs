using Akavache;
using MauiCacheDemo.Core.Enums;
using MauiCacheDemo.Core.Reporting;
using MauiCacheDemo.Core.Settings;
using Splat;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using System.Reactive.Linq;
using XamarinFiles.PdHelpers.Refit.Models;
using static MauiCacheDemo.Core.Enums.DatabaseCacheLocation;
using static MauiCacheDemo.Shared.Constants;
using static System.String;


namespace MauiCacheDemo.Core.Caching
{
    public class DatabaseCacheService : IDatabaseCacheService
    {
        #region Fields - Cache Names and Locations

        // Use adapted from Image Upload sample in https://github.com/RLittlesII/Reactive.Sandbox
        private readonly IBlobCache[] _databaseCaches =
        [
            BlobCache.LocalMachine,
            BlobCache.Secure,
            BlobCache.UserAccount,
            BlobCache.InMemory
        ];

        // Use adapted from Image Upload sample in https://github.com/RLittlesII/Reactive.Sandbox
        private readonly DatabaseCacheLocation[] _databaseCacheLocations =
        [
            LocalMachine,
            Secure,
            UserAccount,
            InMemory
        ];

        #endregion

        #region Constructor

        public DatabaseCacheService(string cacheName,
            IReportingService? reportingService = null)
        {
            CacheName = cacheName;

            ReportingService = reportingService
                ?? Locator.Current.GetService<IReportingService>()
                ?? new ReportingService();

            Startup();
        }

        #endregion

        #region Public - Properties

        public string CacheName { get; }

        #endregion

        #region Private - Internal Services

        private IReportingService ReportingService { get; }

        #endregion

        #region Public/Private - Get Operations

        // Akavache: Attempt to return an object from the cache. If the item doesn't
        // exist or returns an error, call a Func to return the latest
        // version of an object and insert the result in the cache.
        public async Task<(T?, ProblemReport?)> GetOrFetchObject<T>(
            DatabaseCacheKey keyEnum, Func<Task<(T?, ProblemReport?)>> fetchFunc,
            bool? forceLatest = false, string? keyId = null)
        {
            var cacheLocation =
                DatabaseCacheParameters.GetDatabaseCacheLocation(keyEnum);

            if (forceLatest == true)
                return await FetchAndStoreObjectDirect(keyEnum,
                    cacheLocation, fetchFunc, keyId);

            try
            {
                var cachedObject =
                    await GetObjectDirect<T>(keyEnum, cacheLocation, keyId);

                return (cachedObject, null);
            }
            catch (KeyNotFoundException)
            {
                ReportKeyNotFound(keyEnum, cacheLocation);

                return await FetchAndStoreObjectDirect(keyEnum,
                    cacheLocation, fetchFunc, keyId);
            }
            catch (Exception exception)
            {
                ReportGeneralException(cacheLocation, exception, keyEnum);

                return await FetchAndStoreObjectDirect(keyEnum, cacheLocation,
                    fetchFunc, keyId);
            }
        }

        // TODO Return ProblemDetails for exceptions
        // Akavache: Get an object given a key
        public async Task<T?> GetObject<T>(DatabaseCacheKey keyEnum,
            string? keyId = null)
        {
            var cacheLocation =
                DatabaseCacheParameters.GetDatabaseCacheLocation(keyEnum);

            try
            {
                var cachedObject =
                    await GetObjectDirect<T>(keyEnum, cacheLocation, keyId);

                return cachedObject;
            }
            catch (KeyNotFoundException)
            {
                ReportKeyNotFound(keyEnum, cacheLocation);

                return default;
            }
            catch (Exception exception)
            {
                ReportGeneralException(cacheLocation, exception, keyEnum);

                return default;
            }
        }

        [SuppressMessage("ReSharper", "InvertIf")]
        private async Task<(T?, ProblemReport?)> FetchAndStoreObjectDirect<T>(
            DatabaseCacheKey keyEnum, DatabaseCacheLocation cacheLocation,
            Func<Task<(T?, ProblemReport?)>> fetchFunc, string? keyId = null)
        {
            var keyStr = CreateCacheKey(keyEnum, keyId);

            var msg = $"Fetching data for key '{keyStr}'";
            ReportingService.LogCache(msg);

            try
            {
                (T? fetchedObject, ProblemReport? problemReport)
                    = await fetchFunc.Invoke();

                ReportingService.LogObject<T>(fetchedObject,
                    label: $"Fetch - {keyStr}");

                if (fetchedObject != null && problemReport == null)
                {
                    try
                    {
                        await InvalidateObjectDirect<T>(keyEnum, cacheLocation,
                            keyId);

                        await InsertObjectDirect(keyEnum, cacheLocation,
                            fetchedObject, keyId);
                    }
                    catch (Exception exception)
                    {
                        // "Delete old and add new" failed => report and return new data
                        ReportGeneralException(cacheLocation, exception,
                            keyEnum);
                    }

                    return (fetchedObject, null);
                }

                return (fetchedObject, problemReport);

            }
            catch (Exception exception)
            {
                ReportGeneralException(cacheLocation, exception, keyEnum);

                return default;
            }
        }

        // NOTE Try-catch is in calling method to allow for get-or-fetch logic
        private async Task<T?> GetObjectDirect<T>(DatabaseCacheKey keyEnum,
            DatabaseCacheLocation cacheLocation, string? keyId = null)
        {
            var keyStr = CreateCacheKey(keyEnum, keyId);

            var msg = $"Getting key '{keyStr}' from {cacheLocation}";
            ReportingService.LogCache(msg);

            return cacheLocation switch
            {
                LocalMachine =>
                    await BlobCache.LocalMachine.GetObject<T>(keyStr),
                UserAccount =>
                    await BlobCache.UserAccount.GetObject<T>(keyStr),
                Secure =>
                    await BlobCache.Secure.GetObject<T>(keyStr),
                InMemory =>
                    await BlobCache.InMemory.GetObject<T>(keyStr),
                _ =>
                    throw new ArgumentOutOfRangeException(nameof(cacheLocation),
                        cacheLocation, null)
            };
        }

        #endregion

        #region Public - Insert Operations

        // TODO Return ProblemDetails for exceptions
        // Akavache: Insert a single object
        public async Task<Unit> InsertObject<T>(DatabaseCacheKey keyEnum,
            T value, string? keyId = null)
        {
            var cacheLocation =
                DatabaseCacheParameters.GetDatabaseCacheLocation(keyEnum);

            return await InsertObjectDirect(keyEnum, cacheLocation, value, keyId);
        }

        // TODO Return ProblemDetails for exceptions
        private async Task<Unit> InsertObjectDirect<T>(DatabaseCacheKey keyEnum,
            DatabaseCacheLocation cacheLocation, T value, string? keyId = null)
        {
            try
            {
                var keyStr = CreateCacheKey(keyEnum, keyId);
                var keyExpiration = CalculateCacheExpiration(keyEnum);

                var msg = $"Inserting key '{keyStr}' into {cacheLocation}";
                msg += $" with expiration '{keyExpiration}'";
                ReportingService.LogCache(msg);

                return cacheLocation switch
                {
                    LocalMachine =>
                        await BlobCache.LocalMachine.InsertObject(keyStr, value,
                            keyExpiration),
                    UserAccount =>
                        await BlobCache.UserAccount.InsertObject(keyStr, value,
                            keyExpiration),
                    Secure =>
                        await BlobCache.Secure.InsertObject(keyStr, value,
                            keyExpiration),
                    InMemory =>
                        await BlobCache.InMemory.InsertObject(keyStr, value,
                            keyExpiration),
                    _ =>
                        throw new ArgumentOutOfRangeException(
                            nameof(cacheLocation), cacheLocation, null)
                };
            }
            catch (Exception exception)
            {
                ReportGeneralException(cacheLocation, exception, keyEnum);

                return default;
            }
        }
        #endregion

        #region Public - Update Operations

        // TODO Return ProblemDetails for exceptions
        // Composite of Akavache methods for InvalidateObject and InsertObject
        public async Task<Unit> UpdateObject<T>(DatabaseCacheKey keyEnum,
            T value, string? keyId = null)
        {
            var cacheLocation =
                DatabaseCacheParameters.GetDatabaseCacheLocation(keyEnum);

            try
            {
                var msg = $"Updating key '{keyEnum}' in {cacheLocation}";
                ReportingService.LogCache(msg);

                await InvalidateObjectDirect<T>(keyEnum, cacheLocation, keyId);

                return await InsertObjectDirect(keyEnum, cacheLocation, value,
                    keyId);
            }
            catch (Exception exception)
            {
                ReportGeneralException(cacheLocation, exception, keyEnum);

                return default;
            }
        }

        #endregion

        #region Public - Delete Operations

        // Akavache: Delete a single object
        public async Task<Unit> InvalidateObject<T>(DatabaseCacheKey keyEnum,
            string? keyId = null)
        {
            var cacheLocation =
                DatabaseCacheParameters.GetDatabaseCacheLocation(keyEnum);

            return await InvalidateObjectDirect<T>(keyEnum, cacheLocation,
                keyId);
        }

        private async Task<Unit> InvalidateObjectDirect<T>(
            DatabaseCacheKey keyEnum, DatabaseCacheLocation cacheLocation,
            string? keyId = null)
        {

            try
            {
                var keyStr = CreateCacheKey(keyEnum, keyId);

                var msg = $"Invalidating key '{keyStr}' from {cacheLocation}";
                ReportingService.LogCache(msg);

                return cacheLocation switch
                {
                    LocalMachine =>
                        await BlobCache.LocalMachine.InvalidateObject<T>(keyStr),
                    UserAccount =>
                        await BlobCache.UserAccount.InvalidateObject<T>(keyStr),
                    Secure =>
                        await BlobCache.Secure.InvalidateObject<T>(keyStr),
                    InMemory =>
                        await BlobCache.InMemory.InvalidateObject<T>(keyStr),
                    _ =>
                        throw new ArgumentOutOfRangeException(
                            nameof(cacheLocation), cacheLocation, null)
                };
            }
            catch (Exception exception)
            {
                ReportGeneralException(cacheLocation, exception, keyEnum);

                return default;
            }
        }

        // Akavache: Deletes all items (regardless if they are objects or not)
        public IObservable<Unit> InvalidateAll(
            DatabaseCacheLocation? cacheLocation = null)
        {
            if (cacheLocation != null)
                return InvalidateAllDirect((DatabaseCacheLocation)cacheLocation);

            return _databaseCacheLocations
                .Select(InvalidateAllDirect)
                .Merge();
        }

        private IObservable<Unit> InvalidateAllDirect(
            DatabaseCacheLocation cacheLocation)
        {
            try
            {
                PrintKeysByCache(cacheLocation);

                var msg = $"Invalidating all keys in {cacheLocation}";
                ReportingService.LogCache(msg);

                switch (cacheLocation)
                {
                    case LocalMachine:
                        BlobCache.LocalMachine.InvalidateAll();
                        FlushCache(BlobCache.LocalMachine).Wait();
                        VacuumCache(BlobCache.LocalMachine).Wait();

                        break;
                    case UserAccount:
                        BlobCache.UserAccount.InvalidateAll();
                        FlushCache(BlobCache.UserAccount).Wait();
                        VacuumCache(BlobCache.UserAccount).Wait();

                        break;
                    case Secure:
                        BlobCache.Secure.InvalidateAll();
                        FlushCache(BlobCache.Secure).Wait();
                        VacuumCache(BlobCache.Secure).Wait();

                        break;
                    case InMemory:
                        BlobCache.InMemory.InvalidateAll();
                        FlushCache(BlobCache.InMemory).Wait();
                        VacuumCache(BlobCache.InMemory).Wait();

                        break;
                    default:
                        throw new ArgumentOutOfRangeException(
                            nameof(cacheLocation), cacheLocation, null);
                }

                PrintKeysByCache(cacheLocation);
            }
            catch (Exception exception)
            {
                ReportGeneralException(cacheLocation, exception);
            }

            return Observable.Return(Unit.Default);
        }

        #endregion

        #region Public/Private - Maintenance Operations

        private void Startup()
        {
            try
            {
                ReportingService.LogCache("Service Starting ...", CacheName);

                Registrations.Start(CacheName);

                BlobCache.EnsureInitialized();

                // DEBUG Clean up old cache after code change
                //Secure.InvalidateAll().Wait();

                ReportingService.LogCache("Service Started", CacheName);

                PrintKeys();
            }
            catch (Exception exception)
            {
                exception.Data.Add("cacheName", CacheName);

                ReportingService.ReportException(exception);
            }
        }

        public void Shutdown()
        {
            try
            {
                ReportingService.LogCache("Service Shutdown Started",
                    CacheName);

                ShutdownEachCache();

                ReportingService.LogCache("Service Shutdown Completed",
                    CacheName);
            }
            catch (Exception exception)
            {
                ReportingService.ReportException(exception);
            }
        }

        private void ShutdownEachCache()
        {
            _databaseCaches
                .Select(ShutdownOneCache)
                .Merge()
                .Wait();

            BlobCache.Shutdown().Wait();
        }

        private IObservable<Unit> ShutdownOneCache(IBlobCache cache)
        {
            FlushCache(cache);

            // TODO Move Vacuum to start up
            // VacuumCache(cache);

            return ShutdownCache(cache);
        }

        private IObservable<Unit> FlushCache(IBlobCache cache)
        {
            ReportingService.LogCache(Indent + $"Flushing {cache} cache");

            return cache.Flush();
        }

        private IObservable<Unit> ShutdownCache(IBlobCache cache)
        {
            ReportingService.LogCache(Indent + $"Shutting down {cache} cache");

            cache.Dispose();

            return cache.Shutdown;
        }

        private IObservable<Unit> VacuumCache(IBlobCache cache)
        {
            ReportingService.LogCache(Indent + $"Vacuuming {cache} cache");

            return cache.Vacuum();
        }

        #endregion

        #region Private - Helper Methods

        private static DateTimeOffset CalculateCacheExpiration(
            DatabaseCacheKey keyEnum)
        {
            var expirationMinutes =
                DatabaseCacheParameters.GetDatabaseExpirationMinutes(keyEnum);

            var expirationDateTime =
                expirationMinutes == int.MaxValue
                    ? DateTimeOffset.MaxValue
                    : DateTimeOffset.Now.AddMinutes(expirationMinutes);

            return expirationDateTime;
        }

        private static string CreateCacheKey(DatabaseCacheKey keyEnum,
            string? keyId = null)
        {
            var keyStr = keyEnum.ToString();

            if (!IsNullOrWhiteSpace(keyId))
                keyStr += $"-{keyId}";

            return keyStr;
        }

        private void ReportGeneralException(DatabaseCacheLocation cacheLocation,
            Exception exception, DatabaseCacheKey? cacheKey = null)
        {
            exception.Data.Add("cache_location", cacheLocation);

            if (cacheKey != null)
                exception.Data.Add("cache_key", cacheKey);

            ReportingService.ReportException(exception);
        }

        private void ReportKeyNotFound(DatabaseCacheKey cacheKey,
            DatabaseCacheLocation cacheLocation)
        {
            var msg = $"'{cacheKey}' key not found in {cacheLocation} cache";

            ReportingService.LogCache(msg);
        }

        #endregion

        #region Private - Debugging

        private static void PrintKeys(DatabaseCacheLocation? location = null)
        {
            PrintKeysByCache(location);
        }

        [Conditional("DEBUG")]
        private static void PrintKeysByCache(DatabaseCacheLocation? location)
        {
            if (location is null or LocalMachine)
            {
                PrintKeysToDebug(BlobCache.LocalMachine.GetAllKeys().Wait(),
                    "Local Machine");
            }

            if (location is null or UserAccount)
            {
                PrintKeysToDebug(BlobCache.UserAccount.GetAllKeys().Wait(),
                    "User Account");
            }

            if (location is null or Secure)
            {
                PrintKeysToDebug(BlobCache.Secure.GetAllKeys().Wait(),
                    "Secure");
            }

            if (location is null or InMemory)
            {
                PrintKeysToDebug(BlobCache.InMemory.GetAllKeys().Wait(),
                    "In Memory");
            }
        }

        [Conditional("DEBUG")]
        private static void PrintKeysToDebug(IEnumerable<string> keys,
            string location)
        {
            var keyList = (IList<string>) keys;

            if (keyList.Count <= 0) return;

            Debug.WriteLine($"Cache - {location}:");

            foreach (var key in keys)
            {
                Debug.WriteLine($"\t{key}");
            }
        }

        #endregion
    }
}
