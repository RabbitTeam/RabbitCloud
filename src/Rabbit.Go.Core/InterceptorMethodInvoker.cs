using Rabbit.Go.Interceptors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace Rabbit.Go.Core
{
    public interface IMethodInvoker
    {
        Task<object> InvokeAsync(object[] arguments);
    }

    public abstract class InterceptorMethodInvoker : IMethodInvoker
    {
        private readonly IReadOnlyList<IInterceptorMetadata> _interceptors;

        protected InterceptorMethodInvoker(IReadOnlyList<IInterceptorMetadata> interceptors)
        {
            _interceptors = interceptors;
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

        protected abstract Task<RequestMessageBuilder> CreateRequestBuilderAsync(object[] arguments);

        private RequestExecutionDelegate GetRequestExecutionDelegate(RequestMessageBuilder requestBuilder, object[] arguments)
        {
            var requestExecutingContext = new RequestExecutingContext(requestBuilder);
            var requestExecutedContext = new RequestExecutedContext(requestBuilder);

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
                        if (requestExecutingContext.Result != null)
                            requestExecutedContext.Result = requestExecutingContext.Result;
                        return requestExecutedContext;
                    };
                });
            }

            RequestExecutionDelegate requestInvoker = async () =>
            {
                /*var syncRequestInterceptors = _interceptors.OfType<IRequestInterceptor>().ToArray();
                foreach (var interceptor in syncRequestInterceptors)
                    interceptor.OnRequestExecuting(requestExecutingContext);*/

                //                var requestMessage = CreateRequestMessage(requestContext);
                try
                {
                    //                    var responseMessage = await _client.ExecuteAsync(requestMessage, _options);

                    requestExecutedContext.Result = await DoInvokeAsync(requestBuilder.Build(), arguments);
                }
                /*catch (HttpRequestException requestException)
                {
                    throw GoException.ErrorExecuting(requestMessage, requestException);
                }*/
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

        protected abstract Task<object> DoInvokeAsync(HttpRequestMessage requestMessage, object[] arguments);

        #region Implementation of IMethodInvoker

        public virtual async Task<object> InvokeAsync(object[] arguments)
        {
            var requestBuilder = await CreateRequestBuilderAsync(arguments);
            var requestExecutionDelegate = GetRequestExecutionDelegate(requestBuilder, arguments);

            RequestExecutedContext requestExecutedContext = null;
            try
            {
                requestExecutedContext = await requestExecutionDelegate();
                Rethrow(requestExecutedContext);
                return requestExecutedContext?.Result;
            }
            catch (Exception e)
            {
                var exceptionInterceptorContext = new ExceptionInterceptorContext(requestBuilder)
                {
                    ExceptionDispatchInfo = ExceptionDispatchInfo.Capture(e),
                    Result = requestExecutedContext?.Result
                };

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

        #endregion Implementation of IMethodInvoker
    }
}