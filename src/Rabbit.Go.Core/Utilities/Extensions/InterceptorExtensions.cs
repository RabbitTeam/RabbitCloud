using Rabbit.Go.Core.Interceptors;
using Rabbit.Go.Interceptors;
using System;
using System.Threading.Tasks;

namespace Rabbit.Go
{
    public static class InterceptorExtensions
    {
        public static InterceptorCollection AddRequestExecution(this InterceptorCollection interceptors, Func<RequestExecutingContext, RequestExecutionDelegate, Task> execution)
        {
            interceptors.Add(new SimpleInterceptor(execution));
            return interceptors;
        }

        public static InterceptorCollection AddRequestExecuting(this InterceptorCollection interceptors, Func<RequestExecutingContext, Task> invoker)
        {
            interceptors.AddRequestExecution(async (context, next) =>
            {
                await invoker(context);
                await next();
            });
            return interceptors;
        }

        public static InterceptorCollection AddRequestExecuted(this InterceptorCollection interceptors, Func<RequestExecutedContext, Task> invoker)
        {
            interceptors.AddRequestExecution(async (_, next) =>
            {
                var context = await next();
                await invoker(context);
            });
            return interceptors;
        }

        public static InterceptorCollection AddException(this InterceptorCollection interceptors, Func<ExceptionInterceptorContext, Task> invoker)
        {
            interceptors.Add(new SimpleInterceptor(invoker));
            return interceptors;
        }

        private class SimpleInterceptor : IAsyncRequestInterceptor, IAsyncExceptionInterceptor
        {
            private readonly Func<RequestExecutingContext, RequestExecutionDelegate, Task> _execution;
            private readonly Func<ExceptionInterceptorContext, Task> _exception;

            public SimpleInterceptor(Func<RequestExecutingContext, RequestExecutionDelegate, Task> execution) : this(execution, null)
            {
                _execution = execution;
            }

            public SimpleInterceptor(Func<ExceptionInterceptorContext, Task> exception) : this(null, exception)
            {
                _exception = exception;
            }

            private SimpleInterceptor(Func<RequestExecutingContext, RequestExecutionDelegate, Task> execution, Func<ExceptionInterceptorContext, Task> exception)
            {
                _execution = execution;
                _exception = exception;
            }

            #region Implementation of IAsyncRequestInterceptor

            public Task OnRequestExecutionAsync(RequestExecutingContext context, RequestExecutionDelegate next)
            {
                return _execution != null ? _execution(context, next) : next();
            }

            #endregion Implementation of IAsyncRequestInterceptor

            #region Implementation of IAsyncExceptionInterceptor

            public Task OnExceptionAsync(ExceptionInterceptorContext context)
            {
                return _exception == null ? Task.CompletedTask : _exception(context);
            }

            #endregion Implementation of IAsyncExceptionInterceptor
        }
    }
}