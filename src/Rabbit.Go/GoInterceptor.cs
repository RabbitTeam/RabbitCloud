using Castle.DynamicProxy;
using Rabbit.Go.Abstractions;
using Rabbit.Go.Internal;
using Rabbit.Go.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Rabbit.Go
{
    public abstract class GoInterceptor : IInterceptor
    {
        public class InterceptContext
        {
            public InterceptContext(IInvocation invocation)
            {
                Invocation = invocation;
                ProxyType = GetProxyType(invocation);

                Arguments = invocation.MappingArguments();

                var returnType = invocation.Method.ReturnType;

                IsTask = typeof(Task).IsAssignableFrom(returnType);
                HasReturnValue = returnType != typeof(void) && returnType != typeof(Task);

                if (HasReturnValue)
                {
                    if (IsTask)
                    {
                        returnType = returnType.GenericTypeArguments[0];
                    }
                }

                ReturnType = returnType;
            }

            public IInvocation Invocation { get; }
            public IDictionary<string, object> Arguments { get; }
            public Type ProxyType { get; }
            public IGoRequestInvoker Invoker { get; set; }
            public bool IsTask { get; }
            public bool HasReturnValue { get; }
            public Type ReturnType { get; }

            private static Type GetProxyType(IInvocation invocation)
            {
                //todo: think of a more reliable way
                var proxyType = invocation.Proxy.GetType();
                var proxyTypeName = proxyType.Name.Substring(0, proxyType.Name.Length - 5);
                return proxyType.GetInterface(proxyTypeName);
            }
        }

        #region Protected Method

        protected abstract IGoRequestInvoker CreateServiceInvoker(InterceptContext interceptContext);

        protected virtual InterceptContext CreateInterceptContext(IInvocation invocation)
        {
            var interceptContext = new InterceptContext(invocation);

            interceptContext.Invoker = CreateServiceInvoker(interceptContext);

            return interceptContext;
        }

        protected virtual async Task<object> DoHandleAsync(IGoRequestInvoker invoker)
        {
            await invoker.InvokeAsync();

            if (invoker is GoInvoker goInvoker)
                return goInvoker.RequestContext.RabbitContext.Response.Body;

            return null;
        }

        #endregion Protected Method

        #region Implementation of IInterceptor

        public void Intercept(IInvocation invocation)
        {
            var interceptContext = CreateInterceptContext(invocation);

            if (interceptContext.IsTask)
            {
                var serviceInvoker = interceptContext.Invoker;
                if (interceptContext.HasReturnValue)
                {
                    var handler = Cache.GetHandler(interceptContext.ReturnType);
                    invocation.ReturnValue = handler(this, serviceInvoker);
                }
                else
                {
                    invocation.ReturnValue = HandleTaskAsync(serviceInvoker);
                }
            }
            else
            {
                invocation.ReturnValue = Handle(interceptContext.Invoker);
            }
        }

        #endregion Implementation of IInterceptor

        #region Private Method

        private async Task HandleTaskAsync(IGoRequestInvoker invoker)
        {
            await DoHandleAsync(invoker);
        }

        private async Task<T> HandleAsync<T>(IGoRequestInvoker invoker)
        {
            var value = await DoHandleAsync(invoker);
            if (value is Task<T> task)
                return await task;
            return (T)value;
        }

        private object Handle(IGoRequestInvoker invoker)
        {
            return DoHandleAsync(invoker).GetAwaiter().GetResult();
        }

        #endregion Private Method

        #region Help Type

        private static class Cache
        {
            #region Field

            private static readonly ConcurrentDictionary<Type, Func<GoInterceptor, IGoRequestInvoker, Task>> Caches = new ConcurrentDictionary<Type, Func<GoInterceptor, IGoRequestInvoker, Task>>();

            #endregion Field

            public static Func<GoInterceptor, IGoRequestInvoker, Task> GetHandler(Type returnType)
            {
                var key = returnType;

                if (Caches.TryGetValue(key, out var handler))
                    return handler;

                var interceptorParameterExpression = Expression.Parameter(typeof(GoInterceptor), "interceptor");
                var parameterExpression = Expression.Parameter(typeof(IGoRequestInvoker), "invoker");

                var method = typeof(GoInterceptor).GetMethod(nameof(HandleAsync), BindingFlags.NonPublic | BindingFlags.Instance).MakeGenericMethod(returnType);

                var callExpression = Expression.Call(interceptorParameterExpression, method, parameterExpression);

                handler = Expression.Lambda<Func<GoInterceptor, IGoRequestInvoker, Task>>(callExpression, interceptorParameterExpression, parameterExpression).Compile();

                Caches.TryAdd(key, handler);

                return handler;
            }
        }

        #endregion Help Type
    }
}