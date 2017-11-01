using Castle.DynamicProxy;
using Rabbit.Cloud.Client.Abstractions;
using System;
using System.Collections.Generic;
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

            private static readonly IDictionary<object, object> Caches = new Dictionary<object, object>();
            private static readonly ParameterExpression InvocationParameterExpression;
            private static readonly ParameterExpression InstanceParameterExpression;

            #endregion Field

            #region Constructor

            static Cache()
            {
                InvocationParameterExpression = Expression.Parameter(typeof(IInvocation));
                InstanceParameterExpression = Expression.Parameter(typeof(RabbitProxyInterceptor), "instance");
            }

            #endregion Constructor

            public static Delegate GetHandleDelegate(Type returnType)
            {
                var key = ("HandleDelegate", returnType);

                return GetCache(key, () =>
                {
                    var callExpression = Expression.Call(InstanceParameterExpression, nameof(HandleAsync), new[] { returnType }, InvocationParameterExpression);
                    return Expression.Lambda(callExpression, InstanceParameterExpression, InvocationParameterExpression).Compile();
                });
            }

            #region Private Method

            private static T GetCache<T>(object key, Func<T> factory)
            {
                if (Caches.TryGetValue(key, out var cache))
                {
                    return (T)cache;
                }
                return (T)(Caches[key] = factory());
            }

            #endregion Private Method
        }

        #endregion Help Type

        #region Implementation of IInterceptor

        public void Intercept(IInvocation invocation)
        {
            var returnType = invocation.Method.ReturnType;
            var isTask = typeof(Task).IsAssignableFrom(returnType);

            if (isTask)
            {
                returnType = Rabbit.Cloud.Abstractions.Utilities.ReflectionUtilities.GetRealType(returnType);
                var handleDelegate = Cache.GetHandleDelegate(returnType);

                invocation.ReturnValue = handleDelegate.DynamicInvoke(this, invocation);
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