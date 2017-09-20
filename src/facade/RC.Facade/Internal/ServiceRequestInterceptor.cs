using Castle.DynamicProxy;
using Rabbit.Cloud.Abstractions;
using Rabbit.Cloud.Facade.Abstractions.Filters;
using Rabbit.Cloud.Facade.Abstractions.Formatters;
using Rabbit.Cloud.Facade.Features;
using Rabbit.Cloud.Facade.Utilities;
using Rabbit.Cloud.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Facade.Internal
{
    internal class ServiceRequestInterceptor : IInterceptor
    {
        #region Field

        private readonly RabbitRequestDelegate _rabbitRequestDelegate;
        private readonly FacadeOptions _facadeOptions;
        private static readonly MethodInfo HandleAsyncMethodInfo = typeof(ServiceRequestInterceptor).GetMethod(nameof(HandleAsync), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static);

        #endregion Field

        #region Constructor

        public ServiceRequestInterceptor(RabbitRequestDelegate rabbitRequestDelegate, FacadeOptions facadeOptions)
        {
            _rabbitRequestDelegate = rabbitRequestDelegate;
            _facadeOptions = facadeOptions;
        }

        #endregion Constructor

        #region Implementation of IInterceptor

        public void Intercept(IInvocation invocation)
        {
            var returnType = invocation.Method.ReturnType;
            var isTask = typeof(Task).IsAssignableFrom(returnType);

            if (isTask)
            {
                returnType = returnType.GenericTypeArguments.FirstOrDefault() ?? typeof(object);
                invocation.ReturnValue = HandleAsyncMethodInfo.MakeGenericMethod(returnType).Invoke(this, new object[] { invocation });
            }
            else
            {
                invocation.ReturnValue = Handle(invocation);
            }
        }

        #endregion Implementation of IInterceptor

        #region Private Method

        private async Task<T> HandleAsync<T>(IInvocation invocation)
        {
            return (T)await InternalHandleAsync(invocation, typeof(T));
        }

        private object Handle(IInvocation invocation)
        {
            return InternalHandleAsync(invocation, invocation.Method.ReturnType).GetAwaiter().GetResult();
        }

        private async Task<object> InternalHandleAsync(IInvocation invocation, Type returnType)
        {
            // build RabbitContext
            var context = GetRabbitContext(invocation);

            var method = invocation.Method;

            // send service request
            await RequestAsync(method, context);

            // read result from resposne
            return await ReturnAsync(method, context, returnType);
        }

        private static RabbitContext GetRabbitContext(IInvocation invocation)
        {
            var context = new DefaultRabbitContext();
            context.Features.Set<IInvocationFeature>(new InvocationFeature(invocation));
            return context;
        }

        private async Task RequestAsync(MethodInfo method, RabbitContext context)
        {
            var requestFilters = method.GetFilters<IRequestFilter>(context.RequestServices).Cast<IFilterMetadata>().ToArray();
            var exceptionFilters = method.GetFilters<IExceptionFilter>(context.RequestServices).Cast<IFilterMetadata>().ToArray();

            var exceptionContext = new ExceptionContext(context, exceptionFilters);
            try
            {
                OnRequestExecuting(new RequestExecutingContext(context, requestFilters, new Dictionary<string, object>()));
                await _rabbitRequestDelegate(context);
            }
            catch (Exception e)
            {
                exceptionContext.Exception = e;
                exceptionContext.ExceptionDispatchInfo = ExceptionDispatchInfo.Capture(e);
                OnException(exceptionContext);
            }
            finally
            {
                var requestExecutedContext = new RequestExecutedContext(context, requestFilters);
                if (!exceptionContext.ExceptionHandled && exceptionContext.Exception != null)
                {
                    requestExecutedContext.Exception = exceptionContext.Exception;
                    requestExecutedContext.ExceptionDispatchInfo = exceptionContext.ExceptionDispatchInfo;
                }

                OnRequestExecuted(requestExecutedContext);
            }

            if (!exceptionContext.ExceptionHandled && exceptionContext.Exception != null)
                exceptionContext.ExceptionDispatchInfo.Throw();
        }

        private async Task<object> ReturnAsync(MethodInfo method, RabbitContext context, Type returnType)
        {
            var exceptionFilters = method.GetFilters<IExceptionFilter>(context.RequestServices).Cast<IFilterMetadata>().ToArray();
            var resultFilters = method.GetFilters<IResultFilter>(context.RequestServices).Cast<IFilterMetadata>().ToArray();

            var exceptionContext = new ExceptionContext(context, exceptionFilters);
            var resultExecutingContext = new ResultExecutingContext(context, resultFilters, returnType);
            var resultExecutedContext = new ResultExecutedContext(context, resultFilters, returnType);

            try
            {
                var responseMessage = context.Response.ResponseMessage;

                OnResultExecuting(resultExecutingContext);

                using (var stream = await responseMessage.Content.ReadAsStreamAsync())
                {
                    var formatterContext = new OutputFormatterContext(context, returnType, stream);

                    var formatters = _facadeOptions.OutputFormatters.Where(f => f.CanWriteResult(formatterContext)).ToArray();

                    if (!formatters.Any())
                        throw new NotSupportedException("not find formatter.");

                    foreach (var formatter in formatters)
                    {
                        var result = await formatter.WriteAsync(formatterContext);
                        if (!result.IsModelSet)
                            continue;
                        resultExecutedContext.Result = result.Model;
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                exceptionContext.Exception = e;
            }
            finally
            {
                if (!exceptionContext.ExceptionHandled)
                {
                    resultExecutedContext.Exception = exceptionContext.Exception;
                    resultExecutedContext.ExceptionDispatchInfo = exceptionContext.ExceptionDispatchInfo;
                }
                OnResultExecuted(resultExecutedContext);
            }

            if (!exceptionContext.ExceptionHandled && exceptionContext.Exception != null)
                exceptionContext.ExceptionDispatchInfo.Throw();

            return resultExecutedContext.Result;
        }

        private static void OnResultExecuting(ResultExecutingContext context)
        {
            foreach (var resultFilter in context.Filters.Cast<IResultFilter>())
            {
                if (context.Cancel)
                    return;
                resultFilter.OnResultExecuting(context);
            }
        }

        private static void OnResultExecuted(ResultExecutedContext context)
        {
            foreach (var resultFilter in context.Filters.Cast<IResultFilter>())
            {
                if (context.Canceled)
                    return;
                resultFilter.OnResultExecuted(context);
            }
        }

        private static void OnException(ExceptionContext context)
        {
            foreach (var exceptionFilter in context.Filters.Cast<IExceptionFilter>())
            {
                exceptionFilter.OnException(context);
            }
        }

        private static void OnRequestExecuting(RequestExecutingContext context)
        {
            foreach (var requestFilter in context.Filters.Cast<IRequestFilter>())
                requestFilter.OnRequestExecuting(context);
        }

        private static void OnRequestExecuted(RequestExecutedContext context)
        {
            foreach (var requestFilter in context.Filters.Cast<IRequestFilter>())
            {
                if (context.Canceled)
                    return;
                requestFilter.OnRequestExecuted(context);
            }
        }

        #endregion Private Method
    }
}