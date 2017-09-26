using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Rabbit.Cloud.Abstractions;
using Rabbit.Cloud.Facade.Abstractions.Filters;
using Rabbit.Cloud.Facade.Abstractions.Formatters;
using Rabbit.Cloud.Facade.Features;
using Rabbit.Cloud.Facade.Utilities.Extensions;
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
        private readonly IServiceProvider _services;
        private readonly FacadeOptions _facadeOptions;
        private static readonly MethodInfo HandleAsyncMethodInfo = typeof(ServiceRequestInterceptor).GetMethod(nameof(HandleAsync), BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static);

        #endregion Field

        #region Constructor

        public ServiceRequestInterceptor(RabbitRequestDelegate rabbitRequestDelegate, IServiceProvider services)
        {
            _rabbitRequestDelegate = rabbitRequestDelegate;
            _services = services;
            _facadeOptions = _services.GetRequiredService<IOptions<FacadeOptions>>().Value;
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

            var serviceRequestInterceptorContext = new ServiceRequestInterceptorContext(method, context, returnType);
            // send service request
            await RequestAsync(serviceRequestInterceptorContext);

            // read result from resposne
            return await ReturnAsync(serviceRequestInterceptorContext);
        }

        private RabbitContext GetRabbitContext(IInvocation invocation)
        {
            var context = new DefaultRabbitContext();
            context.Features.Set<IInvocationFeature>(new InvocationFeature(invocation));
            var serviceDescriptor = _services.GetRequiredService<IServiceDescriptorCollectionProvider>().ServiceDescriptors.GetServiceDescriptor(invocation.Method.GetHashCode());
            context.Features.Set<IServiceDescriptorFeature>(new ServiceDescriptorFeature(serviceDescriptor));
            return context;
        }

        private async Task RequestAsync(ServiceRequestInterceptorContext context)
        {
            var rabbitContext = context.RabbitContext;
            var exceptionContext = context.ExceptionContext;
            var requestExecutingContext = context.RequestExecutingContext;
            var requestExecutedContext = context.RequestExecutedContext;
            try
            {
                OnRequestExecuting(requestExecutingContext);
                await _rabbitRequestDelegate(rabbitContext);
            }
            catch (Exception e)
            {
                exceptionContext.ExceptionDispatchInfo = ExceptionDispatchInfo.Capture(e);
            }
            finally
            {
                if (!exceptionContext.ExceptionHandled && exceptionContext.Exception != null)
                {
                    requestExecutedContext.Exception = exceptionContext.Exception;
                    requestExecutedContext.ExceptionDispatchInfo = exceptionContext.ExceptionDispatchInfo;
                }

                OnRequestExecuted(requestExecutedContext);

                exceptionContext.ExceptionHandled = requestExecutedContext.ExceptionHandled;
                exceptionContext.ExceptionDispatchInfo = requestExecutedContext.ExceptionDispatchInfo;
            }
        }

        private async Task<object> ReturnAsync(ServiceRequestInterceptorContext context)
        {
            var rabbitContext = context.RabbitContext;
            var exceptionContext = context.ExceptionContext;

            var resultExecutingContext = context.ResultExecutingContext;
            var resultExecutedContext = context.ResultExecutedContext;
            var returnType = context.ReturnType;
            try
            {
                if (exceptionContext.ExceptionHandled || exceptionContext.ExceptionDispatchInfo == null)
                {
                    var responseMessage = rabbitContext.Response.ResponseMessage;

                    OnResultExecuting(resultExecutingContext);

                    responseMessage.EnsureSuccessStatusCode();

                    using (var stream = await responseMessage.Content.ReadAsStreamAsync())
                    {
                        var formatterContext = new OutputFormatterContext(rabbitContext, returnType, stream);

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
            }
            catch (Exception e)
            {
                exceptionContext.ExceptionDispatchInfo = ExceptionDispatchInfo.Capture(e);
            }
            finally
            {
                if (!exceptionContext.ExceptionHandled)
                {
                    resultExecutedContext.Exception = exceptionContext.Exception;
                    resultExecutedContext.ExceptionDispatchInfo = exceptionContext.ExceptionDispatchInfo;
                }
                OnResultExecuted(resultExecutedContext);

                exceptionContext.ExceptionHandled = resultExecutedContext.ExceptionHandled;
                exceptionContext.ExceptionDispatchInfo = resultExecutedContext.ExceptionDispatchInfo;
            }
            if (!exceptionContext.ExceptionHandled)
            {
                OnException(exceptionContext);
            }
            if (!exceptionContext.ExceptionHandled)
                exceptionContext.ExceptionDispatchInfo?.Throw();

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

        #region Help Type

        public class ServiceRequestInterceptorContext
        {
            public MethodInfo MethodInfo { get; }
            public RabbitContext RabbitContext { get; }
            public Abstractions.ServiceDescriptor ServiceDescriptor { get; }
            public Type ReturnType { get; }

            public ServiceRequestInterceptorContext(MethodInfo methodInfo, RabbitContext rabbitContext, Type returnType)
            {
                MethodInfo = methodInfo;
                RabbitContext = rabbitContext;
                ServiceDescriptor = rabbitContext.Features.Get<IServiceDescriptorFeature>().ServiceDescriptor;
                ReturnType = returnType;

                IList<IFilterMetadata> GetFilters<T>() where T : IFilterMetadata
                {
                    return ServiceDescriptor.FilterDescriptors.OrderBy(i => i.Order).Select(i => i.Filter).OfType<T>()
                        .OfType<IFilterMetadata>().ToArray();
                }
                ExceptionContext = new ExceptionContext(rabbitContext, GetFilters<IExceptionFilter>());
                var requestFilters = GetFilters<IRequestFilter>();
                RequestExecutingContext = new RequestExecutingContext(rabbitContext, requestFilters, new Dictionary<string, object>());
                RequestExecutedContext = new RequestExecutedContext(rabbitContext, requestFilters);
                var resultFilters = GetFilters<IResultFilter>();
                ResultExecutingContext = new ResultExecutingContext(rabbitContext, resultFilters, returnType);
                ResultExecutedContext = new ResultExecutedContext(rabbitContext, resultFilters, returnType);
            }

            public ExceptionContext ExceptionContext { get; }
            public RequestExecutingContext RequestExecutingContext { get; }
            public RequestExecutedContext RequestExecutedContext { get; }
            public ResultExecutingContext ResultExecutingContext { get; }
            public ResultExecutedContext ResultExecutedContext { get; }
        }

        #endregion Help Type
    }
}