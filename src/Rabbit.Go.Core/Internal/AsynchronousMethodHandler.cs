using Rabbit.Go.Codec;
using Rabbit.Go.Interceptors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;

namespace Rabbit.Go
{
    public class AsynchronousMethodHandler
    {
        private readonly IGoClient _client;
        private readonly MethodDescriptor _descriptor;
        private readonly Func<object[], Task<RequestContext>> _requestContextFactory;
        private readonly RequestOptions _options;
        private readonly IDecoder _decoder;
        private readonly IEnumerable<IInterceptorMetadata> _interceptors;
        private readonly Func<IRetryer> _retryerFactory;

        public AsynchronousMethodHandler(
            IGoClient client,
            MethodDescriptor descriptor,
            Func<object[], Task<RequestContext>> requestContextFactory,
            RequestOptions options,
            IDecoder decoder,
            IEnumerable<IInterceptorMetadata> interceptors,
            Func<IRetryer> retryerFactory)
        {
            _client = client;
            _descriptor = descriptor;
            _requestContextFactory = requestContextFactory;
            _options = options;
            _decoder = decoder;
            _interceptors = interceptors;
            _retryerFactory = retryerFactory;
        }

        public async Task<object> InvokeAsync(object[] arguments)
        {
            var requestContext = await _requestContextFactory(arguments);
            var requestExecutionDelegate = GetRequestExecutionDelegate(requestContext);

            RequestExecutedContext requestExecutedContext = null;
            try
            {
                var retryer = _retryerFactory();

                RetryableException retryableException = null;
                do
                {
                    try
                    {
                        requestExecutedContext = await requestExecutionDelegate();
                    }
                    catch (RetryableException e)
                    {
                        retryableException = e;
                    }
                } while (retryableException != null && await retryer.IsContinueAsync(retryableException));

                Rethrow(requestExecutedContext);

                return requestExecutedContext?.Result;
            }
            catch (Exception e)
            {
                var exceptionInterceptorContext = new ExceptionInterceptorContext(requestContext)
                {
                    ExceptionDispatchInfo = ExceptionDispatchInfo.Capture(e),
                    Result = requestExecutedContext?.Result
                };

                /*                var syncExceptionInterceptors = _interceptors.OfType<IExceptionInterceptor>().ToArray();
                                foreach (var interceptor in syncExceptionInterceptors)
                                    interceptor.OnException(exceptionInterceptorContext);*/

                var exceptionInterceptors = _interceptors
                    .OfType<IAsyncExceptionInterceptor>()
                    .Reverse()
                    .ToArray();
                foreach (var interceptor in exceptionInterceptors)
                    await interceptor.OnExceptionAsync(exceptionInterceptorContext);

                Rethrow(exceptionInterceptorContext);

                return exceptionInterceptorContext.Result;
            }
        }

        private static void Rethrow(RequestExecutedContext context)
        {
            if (context == null)
                return;

            if (context.ExceptionHandled)
                return;

            context.ExceptionDispatchInfo?.Throw();

            if (context.Exception != null)
                throw context.Exception;
        }

        private static void Rethrow(ExceptionInterceptorContext context)
        {
            if (context == null)
                return;

            if (context.ExceptionHandled)
                return;

            context.ExceptionDispatchInfo?.Throw();

            if (context.Exception != null)
                throw context.Exception;
        }

        private RequestExecutionDelegate GetRequestExecutionDelegate(RequestContext requestContext)
        {
            var requestExecutingContext = new RequestExecutingContext(requestContext);
            var requestExecutedContext = new RequestExecutedContext(requestContext);

            var requestInterceptors = _interceptors
                .OfType<IAsyncRequestInterceptor>()
                .ToArray();
            IList<Func<RequestExecutionDelegate, RequestExecutionDelegate>> requestExecutions = new List<Func<RequestExecutionDelegate, RequestExecutionDelegate>>();

            foreach (var interceptor in requestInterceptors)
            {
                requestExecutions.Add(next =>
                {
                    return async () =>
                    {
                        await interceptor.OnActionExecutionAsync(requestExecutingContext, next);
                        return requestExecutedContext;
                    };
                });
            }

            RequestExecutionDelegate requestInvoker = async () =>
            {
                /*var syncRequestInterceptors = _interceptors.OfType<IRequestInterceptor>().ToArray();
                foreach (var interceptor in syncRequestInterceptors)
                    interceptor.OnRequestExecuting(requestExecutingContext);*/

                var requestMessage = CreateRequestMessage(requestContext);
                try
                {
                    var responseMessage = await _client.ExecuteAsync(requestMessage, _options);

                    requestExecutedContext.Result = await DecodeAsync(responseMessage);
                }
                catch (HttpRequestException requestException)
                {
                    throw GoException.ErrorExecuting(requestMessage, requestException);
                }
                catch (Exception e)
                {
                    requestExecutedContext.ExceptionDispatchInfo = ExceptionDispatchInfo.Capture(e);
                }

                /*foreach (var interceptor in syncRequestInterceptors)
                    interceptor.OnRequestExecuted(requestExecutedContext);*/

                return requestExecutedContext;
            };
            foreach (var func in requestExecutions.Reverse())
            {
                requestInvoker = func(requestInvoker);
            }

            return requestInvoker;
        }

        private async Task<object> DecodeAsync(HttpResponseMessage response)
        {
            try
            {
                return _decoder == null
                    ? null
                    : await _decoder.DecodeAsync(response, _descriptor.ReturnType);
            }
            catch (DecodeException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new DecodeException(e.Message, e);
            }
        }

        private static string BuildQueryString(RequestContext context)
        {
            if (!context.Query.Any())
                return "?";

            var queryBuilder = new StringBuilder();
            foreach (var item in context.Query)
            {
                queryBuilder
                    .Append("&")
                    .Append(item.Key)
                    .Append("=")
                    .Append(item.Value.ToString());
            }

            var queryString = queryBuilder.Remove(0, 1).Insert(0, '?').ToString();
            return queryString;
        }

        private static void BuildUri(HttpRequestMessage message, RequestContext context)
        {
            var queryString = BuildQueryString(context);

            var url = $"{context.Scheme}://{context.Host}:{context.Port}{context.Path}";

            if (queryString != "?")
            {
                url += queryString;
            }

            message.RequestUri = new Uri(url);
        }

        private static void BuildHeaders(HttpRequestMessage message, RequestContext context)
        {
            var messageHeaders = message.Headers;
            var contentHeaders = message.Content.Headers;

            foreach (var item in context.Headers)
            {
                var key = item.Key;
                var values = item.Value.ToArray();

                if (contentHeaders.TryAddWithoutValidation(key, values))
                    continue;
                messageHeaders.Add(key, values);
            }
        }

        private static void BuildContent(HttpRequestMessage message, RequestContext context)
        {
            message.Content = context.Body != null
                ? new ByteArrayContent(context.Body)
                : new StringContent(string.Empty);

            if (context.Charset != null)
                message.Content.Headers.ContentType.CharSet = context.Charset;
        }

        private static void BuildMethod(HttpRequestMessage message, RequestContext context)
        {
            message.Method = GetHttpMethod(context.Method);
        }

        private static HttpRequestMessage CreateRequestMessage(RequestContext context)
        {
            var message = new HttpRequestMessage();

            BuildMethod(message, context);
            BuildUri(message, context);
            BuildContent(message, context);
            BuildHeaders(message, context);

            return message;
        }

        private static HttpMethod GetHttpMethod(string method, HttpMethod def = null)
        {
            switch (method?.ToLower())
            {
                case "delete":
                    return HttpMethod.Delete;

                case "get":
                    return HttpMethod.Get;

                case "head":
                    return HttpMethod.Head;

                case "options":
                    return HttpMethod.Options;

                case "post":
                    return HttpMethod.Post;

                case "put":
                    return HttpMethod.Put;

                case "trace":
                    return HttpMethod.Trace;

                case null:
                case "":
                    return def ?? HttpMethod.Get;

                default:
                    return new HttpMethod(method);
            }
        }
    }
}