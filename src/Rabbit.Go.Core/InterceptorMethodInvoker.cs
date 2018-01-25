using Rabbit.Go.Interceptors;
using System;
using System.Collections.Generic;
using System.Linq;
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
        protected RequestMessageBuilder RequestBuilder { get; }
        private readonly IReadOnlyList<IInterceptorMetadata> _interceptors;

        protected InterceptorMethodInvoker(RequestMessageBuilder requestBuilder, IReadOnlyList<IInterceptorMetadata> interceptors)
        {
            RequestBuilder = requestBuilder;
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

        private RequestExecutionDelegate GetRequestExecutionDelegate(object[] arguments)
        {
            var requestExecutingContext = new RequestExecutingContext(RequestBuilder);
            var requestExecutedContext = new RequestExecutedContext(RequestBuilder);

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

                    requestExecutedContext.Result = await DoInvokeAsync(arguments);
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

        protected abstract Task<object> DoInvokeAsync(object[] arguments);

        #region Implementation of IMethodInvoker

        public virtual async Task<object> InvokeAsync(object[] arguments)
        {
            var requestExecutionDelegate = GetRequestExecutionDelegate(arguments);

            RequestExecutedContext requestExecutedContext = null;
            try
            {
                requestExecutedContext = await requestExecutionDelegate();
                Rethrow(requestExecutedContext);
                return requestExecutedContext?.Result;
            }
            catch (Exception e)
            {
                var exceptionInterceptorContext = new ExceptionInterceptorContext(RequestBuilder)
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