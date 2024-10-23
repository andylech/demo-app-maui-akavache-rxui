using MauiCacheDemo.Core.Enums;
using System.Diagnostics.CodeAnalysis;
using static MauiCacheDemo.Core.Enums.DatabaseCacheLocation;

namespace MauiCacheDemo.Core.Settings;

public static class DatabaseCacheParameters
{
    #region Fields

    private const DatabaseCacheLocation DefaultDatabaseLocation = Secure;

    // TODO Suitable overall default cache expiration limit
    private const int DefaultExpirationMinutes = 5;

    #endregion

    #region Public  - Methods

    public static DatabaseCacheLocation
        GetDatabaseCacheLocation(DatabaseCacheKey cacheKey)
    {
        return DatabaseLocationExceptions.GetValueOrDefault(cacheKey,
            DefaultDatabaseLocation);
    }

    public static int
        GetDatabaseExpirationMinutes(DatabaseCacheKey cacheKey)
    {
        return ExpirationMinutesExceptions.GetValueOrDefault(cacheKey,
            DefaultExpirationMinutes);
    }

    #endregion

    #region Private - Dictionaries

    [SuppressMessage("ReSharper", "CollectionNeverUpdated.Local")]
    private static readonly Dictionary<DatabaseCacheKey, DatabaseCacheLocation>
        DatabaseLocationExceptions =
            new Dictionary<DatabaseCacheKey, DatabaseCacheLocation>
            {
                // TODO Add { <key>, <location> } when need other than Secure
            };

    [SuppressMessage("ReSharper", "CollectionNeverUpdated.Local")]
    private static readonly Dictionary<DatabaseCacheKey, int>
        ExpirationMinutesExceptions =
            new Dictionary<DatabaseCacheKey, int>
            {
                // TODO Add { <key>, <minutes> } when need other than DefaultExpirationMinutes
            };

    #endregion
}
