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
        protected readonly IList<IInterceptorMetadata> Interceptors;
        protected readonly RequestContext RequestContext;

        protected InterceptorMethodInvoker(RequestContext requestContext, IList<IInterceptorMetadata> interceptors)
        {
            RequestContext = requestContext;
            Interceptors = interceptors;
        }

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
                var exceptionInterceptorContext = new ExceptionInterceptorContext(RequestContext, Interceptors)
                {
                    ExceptionDispatchInfo = ExceptionDispatchInfo.Capture(e),
                    Result = requestExecutedContext?.Result
                };

                var exceptionInterceptors = Interceptors
                    .OfType<IAsyncExceptionInterceptor>()
                    .Reverse()
                    .ToArray();
                foreach (var interceptor in exceptionInterceptors)
                    await interceptor.OnExceptionAsync(exceptionInterceptorContext);

                Rethrow(exceptionInterceptorContext);

                return exceptionInterceptorContext.Result;
            }
            finally
            {
                if (RequestContext.GoContext.RequestServices is IDisposable disposable)
                    disposable.Dispose();
            }
        }

        #endregion Implementation of IMethodInvoker

        protected abstract Task<object> DoInvokeAsync(object[] arguments);

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
            var requestExecutingContext = new RequestExecutingContext(RequestContext, Interceptors);
            var requestExecutedContext = new RequestExecutedContext(RequestContext, Interceptors);

            var requestInterceptors = Interceptors
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
                try
                {
                    requestExecutedContext.Result = await DoInvokeAsync(arguments);
                }
                catch (Exception e)
                {
                    requestExecutedContext.ExceptionDispatchInfo = ExceptionDispatchInfo.Capture(e);
                }

                return requestExecutedContext;
            };
            foreach (var func in requestExecutions.Reverse())
            {
                requestInvoker = func(requestInvoker);
            }

            return requestInvoker;
        }
    }
}