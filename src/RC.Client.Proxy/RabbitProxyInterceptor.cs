using Castle.DynamicProxy;
using Rabbit.Cloud.Application.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Client.Proxy
{
    public abstract class RabbitProxyInterceptor : IInterceptor
    {
        private readonly RabbitRequestDelegate _invoker;

        protected abstract IRabbitContext CreateRabbitContext(IInvocation invocation);

        protected abstract Task<object> ConvertReturnValue(IInvocation invocation, IRabbitContext rabbitContext);

        protected RabbitProxyInterceptor(RabbitRequestDelegate invoker)
        {
            _invoker = invoker;
        }

        #region Help Type

        private static class Cache
        {
            #region Field

            private static readonly ConcurrentDictionary<Type, Func<RabbitProxyInterceptor, IInvocation, Task>> Caches = new ConcurrentDictionary<Type, Func<RabbitProxyInterceptor, IInvocation, Task>>();

            #endregion Field

            public static Func<RabbitProxyInterceptor, IInvocation, Task> GetHandler(Type returnType)
            {
                var key = returnType;

                if (Caches.TryGetValue(key, out var handler))
                    return handler;

                var invocationParameterExpression = Expression.Parameter(typeof(IInvocation));
                var instanceParameterExpression = Expression.Parameter(typeof(RabbitProxyInterceptor), "instance");

                var callExpression = Expression.Call(instanceParameterExpression, nameof(HandleAsync), new[] { returnType }, invocationParameterExpression);
                handler = Expression.Lambda<Func<RabbitProxyInterceptor, IInvocation, Task>>(callExpression, instanceParameterExpression, invocationParameterExpression).Compile();

                Caches.TryAdd(key, handler);

                return handler;
            }
        }

        #endregion Help Type

        #region Implementation of IInterceptor

        public void Intercept(IInvocation invocation)
        {
            var returnType = invocation.Method.ReturnType;
            var isTask = typeof(Task).IsAssignableFrom(returnType);

            if (isTask)
            {
                returnType = Abstractions.Utilities.ReflectionUtilities.GetRealType(returnType);
                var handler = Cache.GetHandler(returnType);

                invocation.ReturnValue = handler(this, invocation);
            }
            else
            {
                invocation.ReturnValue = Handle(invocation);
            }
        }

        #endregion Implementation of IInterceptor

        private async Task<T> HandleAsync<T>(IInvocation invocation)
        {
            var value = await InternalHandleAsync(invocation);
            return (T)value;
        }

        private object Handle(IInvocation invocation)
        {
            return InternalHandleAsync(invocation).GetAwaiter().GetResult();
        }

        private async Task<object> InternalHandleAsync(IInvocation invocation)
        {
            var context = CreateRabbitContext(invocation);

            await _invoker(context);

            return await ConvertReturnValue(invocation, context);
        }
    }
}