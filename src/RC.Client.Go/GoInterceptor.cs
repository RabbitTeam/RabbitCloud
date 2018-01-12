using Castle.DynamicProxy;
using Rabbit.Cloud.Client.Go.ApplicationModels;
using Rabbit.Cloud.Client.Go.Internal;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Client.Go
{
    public abstract class GoInterceptor : IInterceptor
    {
        private readonly ApplicationModel _applicationModel;

        public class InterceptContext
        {
            public InterceptContext(IInvocation invocation)
            {
                ProxyType = GetProxyType(invocation);

                var returnType = invocation.Method.ReturnType;
                IsTask = typeof(Task).IsAssignableFrom(returnType);
                HasReturnValue = returnType != typeof(void) && returnType != typeof(Task);
            }

            public IInvocation Invocation { get; set; }
            public Type ProxyType { get; set; }
            public IServiceInvoker ServiceInvoker { get; set; }
            public bool IsTask { get; }
            public bool HasReturnValue { get; }
            public RequestModel RequestModel { get; set; }

            private static Type GetProxyType(IInvocation invocation)
            {
                //todo: think of a more reliable way
                var proxyType = invocation.Proxy.GetType();
                var proxyTypeName = proxyType.Name.Substring(0, proxyType.Name.Length - 5);
                return proxyType.GetInterface(proxyTypeName);
            }
        }

        protected IServiceProvider ServiceProvider { get; }

        protected GoInterceptor(ApplicationModel applicationModel, IServiceProvider serviceProvider)
        {
            _applicationModel = applicationModel;
            ServiceProvider = serviceProvider;
        }

        #region Protected Method

        protected abstract IServiceInvoker CreateServiceInvoker(InterceptContext interceptContext);

        protected virtual InterceptContext CreateInterceptContext(IInvocation invocation)
        {
            var interceptContext = new InterceptContext(invocation)
            {
                Invocation = invocation
            };

            var proxyType = interceptContext.ProxyType;
            var proxyMethod = invocation.Method;

            var requestModel = GetRequestModel(_applicationModel, proxyType, proxyMethod);
            interceptContext.RequestModel = requestModel;

            interceptContext.ServiceInvoker = CreateServiceInvoker(interceptContext);

            return interceptContext;
        }

        protected virtual async Task<object> DoHandleAsync(IServiceInvoker invoker)
        {
            await invoker.InvokeAsync();
            if (!(invoker is ServiceInvoker serviceInvoker))
                return null;

            var value = serviceInvoker.InvokerContext.RequestContext.RabbitContext.Response.Body;
            return value;
        }

        #endregion Protected Method

        #region Implementation of IInterceptor

        public void Intercept(IInvocation invocation)
        {
            var interceptContext = CreateInterceptContext(invocation);

            if (interceptContext.IsTask)
            {
                var serviceInvoker = interceptContext.ServiceInvoker;
                if (interceptContext.HasReturnValue)
                {
                    var handler = Cache.GetHandler(interceptContext.RequestModel.ResponseType);
                    invocation.ReturnValue = handler(this, serviceInvoker);
                }
                else
                {
                    invocation.ReturnValue = HandleTaskAsync(serviceInvoker);
                }
            }
            else
            {
                invocation.ReturnValue = Handle(interceptContext.ServiceInvoker);
            }
        }

        #endregion Implementation of IInterceptor

        #region Private Method

        private async Task HandleTaskAsync(IServiceInvoker serviceInvoker)
        {
            await DoHandleAsync(serviceInvoker);
        }

        private async Task<T> HandleAsync<T>(IServiceInvoker serviceInvoker)
        {
            var value = await DoHandleAsync(serviceInvoker);
            if (value is Task<T> task)
                return await task;
            return (T)value;
        }

        private object Handle(IServiceInvoker serviceInvoker)
        {
            return DoHandleAsync(serviceInvoker).GetAwaiter().GetResult();
        }

        private static RequestModel GetRequestModel(ApplicationModel applicationModel, Type proxyType, MethodInfo proxyMethod)
        {
            return applicationModel.Services
                .Where(i => i.Type == proxyType)
                .SelectMany(i => i.Requests)
                .SingleOrDefault(i => i.MethodInfo == proxyMethod);
        }

        #endregion Private Method

        #region Help Type

        private static class Cache
        {
            #region Field

            private static readonly ConcurrentDictionary<Type, Func<GoInterceptor, IServiceInvoker, Task>> Caches = new ConcurrentDictionary<Type, Func<GoInterceptor, IServiceInvoker, Task>>();

            #endregion Field

            public static Func<GoInterceptor, IServiceInvoker, Task> GetHandler(Type returnType)
            {
                var key = returnType;

                if (Caches.TryGetValue(key, out var handler))
                    return handler;

                var interceptorParameterExpression = Expression.Parameter(typeof(GoInterceptor), "interceptor");
                var parameterExpression = Expression.Parameter(typeof(IServiceInvoker), "serviceInvoker");

                var method = typeof(GoInterceptor).GetMethod(nameof(HandleAsync), BindingFlags.NonPublic | BindingFlags.Instance).MakeGenericMethod(returnType);

                var callExpression = Expression.Call(interceptorParameterExpression, method, parameterExpression);

                handler = Expression.Lambda<Func<GoInterceptor, IServiceInvoker, Task>>(callExpression, interceptorParameterExpression, parameterExpression).Compile();

                Caches.TryAdd(key, handler);

                return handler;
            }
        }

        #endregion Help Type
    }
}