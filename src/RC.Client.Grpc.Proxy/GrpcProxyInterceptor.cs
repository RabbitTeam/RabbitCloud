using Castle.DynamicProxy;
using Grpc.Core;
using Rabbit.Cloud.Client.Abstractions;
using Rabbit.Cloud.Client.Features;
using Rabbit.Cloud.Client.Proxy;
using Rabbit.Cloud.Grpc.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Client.Grpc.Proxy
{
    public class GrpcProxyInterceptor : RabbitProxyInterceptor
    {
        private static readonly Type[] IgnoreGenericTypes = { typeof(AsyncServerStreamingCall<>), typeof(AsyncDuplexStreamingCall<,>) };

        public GrpcProxyInterceptor(RabbitRequestDelegate invoker) : base(invoker)
        {
        }

        #region Overrides of RabbitProxyInterceptor

        protected override IRabbitContext CreateRabbitContext(IInvocation invocation)
        {
            var descriptor = GrpcServiceDescriptor.Create(invocation.Method);

            var context = new GrpcRabbitContext();

            context.Request.Url = new ServiceUrl
            {
                Scheme = "grpc",
                Path = descriptor.ServiceId
            };
            context.Request.Request = invocation.Arguments[0];

            return context;
        }

        protected override async Task<object> ConvertReturnValue(IInvocation invocation, IRabbitContext rabbitContext)
        {
            var response = ((GrpcRabbitContext)rabbitContext).Response.Response;

            var returnType = invocation.Method.ReturnType;

            if (returnType == typeof(void))
                return null;

            var responseType = response.GetType();
            if (IgnoreGenericTypes.Contains(responseType.GetGenericTypeDefinition()))
                return response;

            if (!typeof(Task).IsAssignableFrom(returnType))
                return response;

            var responseAsyncPropertyAccessor = Cache.GetResponseAsyncAccessor(responseType);
            var responseAsync = (Task)responseAsyncPropertyAccessor.DynamicInvoke(response);

            if (!returnType.IsGenericType)
                return responseAsync;

            await responseAsync;

            var taskResultAccessor = Cache.GetTaskResultAccessor(responseAsync);
            return taskResultAccessor.DynamicInvoke(responseAsync);
        }

        #endregion Overrides of RabbitProxyInterceptor

        #region Help Type

        private static class Cache
        {
            #region Field

            private static readonly IDictionary<object, object> Caches = new Dictionary<object, object>();

            #endregion Field

            public static Delegate GetResponseAsyncAccessor(Type type)
            {
                var key = ("ResponseAsyncAccessor", type);

                return GetCache(key, () =>
                {
                    var parameterExpression = GetParameterExpression(type);
                    var responseAsyncPropertyExpression = GetResponseAsyncExpression(parameterExpression);
                    return Expression.Lambda(responseAsyncPropertyExpression, parameterExpression).Compile();
                });
            }

            public static Delegate GetTaskResultAccessor(Task task)
            {
                var type = task.GetType();
                if (!type.IsGenericType)
                    throw new ArgumentException("type is not Task<>.", nameof(type));

                var key = ("TaskResultAccessor", type);

                return GetCache(key, () =>
                {
                    var parameterExpression = Expression.Parameter(type);

                    var getAwaiterMethodInfo = type.GetMethod(nameof(Task<object>.GetAwaiter));

                    var callExpression = Expression.Call(Expression.Call(parameterExpression, getAwaiterMethodInfo), nameof(TaskAwaiter.GetResult), null);

                    return Expression.Lambda(callExpression, parameterExpression).Compile();
                });
            }

            #region Private Method

            private static MemberExpression GetResponseAsyncExpression(ParameterExpression parameterExpression)
            {
                var key = ("ResponseAsyncExpression", parameterExpression.Type);

                return GetCache(key, () =>
                {
                    var propertyExpression = Expression.Property(parameterExpression, nameof(AsyncUnaryCall<object>.ResponseAsync));
                    return propertyExpression;
                });
            }

            private static ParameterExpression GetParameterExpression(Type type)
            {
                var key = ("Parameter", type);
                return GetCache(key, () => Expression.Parameter(type));
            }

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
    }
}