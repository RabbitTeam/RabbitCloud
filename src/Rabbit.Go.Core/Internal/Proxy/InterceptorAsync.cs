using Castle.DynamicProxy;
using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Rabbit.Go
{
    public class InterceptorAsync : IInterceptor
    {
        private readonly Func<IInvocation, Task<object>> _invoker;

        public InterceptorAsync(Func<IInvocation, Task<object>> invoker)
        {
            _invoker = invoker;
        }

        #region Implementation of IInterceptor

        public virtual void Intercept(IInvocation invocation)
        {
            var returnType = invocation.Method.ReturnType;

            var isTask = typeof(Task).IsAssignableFrom(returnType);

            if (returnType == typeof(void))
                returnType = null;
            else if (isTask)
                returnType = returnType.IsGenericType ? returnType.GenericTypeArguments[0] : null;

            object result;
            if (isTask)
            {
                result = returnType == null ? HandleTaskAsync(invocation) : Cache.GetHandler(returnType)(this, invocation);
            }
            else
            {
                result = Handle(invocation);
            }

            if (result != null)
                invocation.ReturnValue = result;
        }

        #endregion Implementation of IInterceptor

        private async Task HandleTaskAsync(IInvocation invocation)
        {
            await DoHandleAsync(invocation);
        }

        private async Task<T> HandleAsync<T>(IInvocation invocation)
        {
            var value = await DoHandleAsync(invocation);

            switch (value)
            {
                case null:
                    return default(T);

                case Task<T> task:
                    return await task;
            }

            return (T)value;
        }

        private object Handle(IInvocation invocation)
        {
            return DoHandleAsync(invocation).GetAwaiter().GetResult();
        }

        private async Task<object> DoHandleAsync(IInvocation invocation)
        {
            return await _invoker(invocation);
        }

        #region Help Type

        private static class Cache
        {
            #region Field

            private static readonly ConcurrentDictionary<Type, Func<InterceptorAsync, IInvocation, Task>> Caches = new ConcurrentDictionary<Type, Func<InterceptorAsync, IInvocation, Task>>();

            #endregion Field

            public static Func<InterceptorAsync, IInvocation, Task> GetHandler(Type returnType)
            {
                var key = returnType;

                if (Caches.TryGetValue(key, out var handler))
                    return handler;

                var interceptorParameterExpression = Expression.Parameter(typeof(InterceptorAsync), "interceptor");
                var parameterExpression = Expression.Parameter(typeof(IInvocation), "invocation");

                var method = typeof(InterceptorAsync).GetMethod(nameof(HandleAsync), BindingFlags.NonPublic | BindingFlags.Instance).MakeGenericMethod(returnType);

                var callExpression = Expression.Call(interceptorParameterExpression, method, parameterExpression);

                handler = Expression.Lambda<Func<InterceptorAsync, IInvocation, Task>>(callExpression, interceptorParameterExpression, parameterExpression).Compile();

                Caches.TryAdd(key, handler);

                return handler;
            }
        }

        #endregion Help Type
    }
}