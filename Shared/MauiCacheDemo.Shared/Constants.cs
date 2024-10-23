using Refit;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MauiCacheDemo.Shared
{
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public static class Constants
    {
        #region Characters

        // TODO Allow option of precise width
        public const string Indent = "\t";

        #endregion

        #region JSON Serialization

        public static readonly JsonSerializerOptions
            ReadJsonOptions = new()
            {
                AllowTrailingCommas = true,
                PropertyNameCaseInsensitive = true
            };

        public static readonly JsonSerializerOptions
            RefitJsonOptions = new()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                NumberHandling = JsonNumberHandling.AllowReadingFromString,
                PropertyNameCaseInsensitive = true
            };

        private static readonly JsonSerializerOptions
            WriteJsonOptionsWithNull = new()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.Never,
#if DEBUG
                WriteIndented = true
#endif
            };

        private static readonly JsonSerializerOptions
            WriteJsonOptionsWithoutNull = new()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
#if DEBUG
                WriteIndented = true
#endif
            };

        public static JsonSerializerOptions
            GetWriteJsonOptions(bool keepNulls)
        {
            return keepNulls
                ? WriteJsonOptionsWithNull
                : WriteJsonOptionsWithoutNull;
        }

        #endregion

        #region Refit

        public static readonly RefitSettings RefitServiceSettings =
            new()
            {
                ContentSerializer =
                    new SystemTextJsonContentSerializer(RefitJsonOptions)
            };

        #endregion
    }
}
