using Refit;
using Splat;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using XamarinFiles.FancyLogger;
using static System.Net.HttpStatusCode;
using static System.Net.Sockets.SocketError;
using static System.Net.WebExceptionStatus;
using static MauiCacheDemo.Shared.Constants;

namespace MauiCacheDemo.Shared
{
    public static class ApiCallWrapper
    {
        #region Fields - Static Logger

        private static readonly IFancyLogger FancyLogger;

        private const string LoggerPrefix = "API Wrapper";

        #endregion

        #region Initialize

        // Warm up the class to set up logger before use
        public static void Initialize() { }

        #endregion

        #region Constructors

        static ApiCallWrapper()
        {
            FancyLogger = Locator.Current.GetService<IFancyLogger>()
                ?? new FancyLogger(allLinesPrefix: LoggerPrefix);
        }

        #endregion

        #region Public - Methods

        public static async Task CallApiEndpoint(Func<Task> apiCallFunc)
        {
            try
            {
                // TODO Add Polly retry logic
                await apiCallFunc.Invoke().ConfigureAwait(false);
            }
            catch (HttpRequestException requestException)
            {
                var methodName = PullApiMethodName(apiCallFunc.Method);

                var exception =
                    await HandleHttpRequestException(requestException,
                        methodName);

                FancyLogger.LogHttpRequestException(requestException);

                throw exception;
            }
        }

        public static async Task<T> CallApiEndpoint<T>(Func<Task<T>> apiCallFunc)
        {
            try
            {
                // TODO Add Polly retry logic
                var result = await apiCallFunc.Invoke().ConfigureAwait(false);

                return result;
            }
            catch (HttpRequestException requestException)
            {
                var methodName = PullApiMethodName(apiCallFunc.Method);

                var exception =
                    await HandleHttpRequestException(requestException,
                        methodName);

                FancyLogger.LogHttpRequestException(requestException);

                throw exception;
            }
        }

        #endregion

        #region Private - Methods

        // Consolidate different network exceptions into Refit ApiException
        private static async Task<ApiException> HandleHttpRequestException(
            HttpRequestException requestException, string methodName)
        {
            // TODO Add more networking error conditions
            HttpStatusCode? statusCode = requestException.InnerException switch
            {
                SocketException { SocketErrorCode: ConnectionRefused }
                    or WebException { Status: NameResolutionFailure }
                    => ServiceUnavailable,
                _ => InternalServerError
            };

            FancyLogger.LogHttpRequestException(requestException);

            // TODO Pull from apiCallFunc
            var requestMessage = new HttpRequestMessage();
            var requestMethod = new HttpMethod(methodName);
            var responseMessage =
                new HttpResponseMessage((HttpStatusCode)statusCode);

            var apiException =
                await ApiException.Create(
                    requestException.Message,
                    requestMessage,
                    requestMethod,
                    responseMessage,
                    RefitServiceSettings,
                    requestException.InnerException);

            FancyLogger.LogApiException(apiException);

            return apiException;
        }

        private static string PullApiMethodName(MethodInfo methodInfo)
        {
            // TODO Pull real HTTP request info from apiCallFunc
            var methodInfoName = methodInfo.Name;
            var methodInfoNameComponents =
                methodInfoName.Split('<', '>');
            var methodName = methodInfoNameComponents[1];

            return methodName;
        }

        #endregion
    }
}
