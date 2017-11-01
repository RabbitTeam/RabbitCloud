using Grpc.Core;
using Rabbit.Cloud.Client.Abstractions;
using Rabbit.Cloud.Client.Features;
using Rabbit.Cloud.Client.Grpc.Features;
using Rabbit.Cloud.Grpc.Abstractions.Method;
using Rabbit.Cloud.Grpc.Client.Extensions;
using Rabbit.Cloud.Grpc.Client.Internal;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Client.Grpc
{
    public class GrpcMiddleware
    {
        private readonly RabbitRequestDelegate _next;
        private readonly CallInvokerPool _callInvokerPool;
        private readonly IMethodCollection _methodCollection;

        public GrpcMiddleware(RabbitRequestDelegate next, CallInvokerPool callInvokerPool, IMethodCollection methodCollection)
        {
            _next = next;
            _callInvokerPool = callInvokerPool;
            _methodCollection = methodCollection;
        }

        public async Task Invoke(IRabbitContext context)
        {
            var grpcRequestFeature = context.Features.Get<IGrpcRequestFeature>();

            grpcRequestFeature.CallOptions = grpcRequestFeature.CallOptions.WithDeadline(DateTime.UtcNow.AddSeconds(5));

            var requestFeature = context.Features.Get<IRequestFeature>();
            var serviceUrl = requestFeature.ServiceUrl;

            var callInvoker = _callInvokerPool.GetCallInvoker(serviceUrl.Host, serviceUrl.Port);

            var serviceId = serviceUrl.Path;
            var method = _methodCollection.Get(serviceId);

            var response = callInvoker.Call(method, grpcRequestFeature.Host, grpcRequestFeature.CallOptions, grpcRequestFeature.Request);

            var getAwaiterMethod = response.GetType().GetMethod("GetAwaiter");

            var getResult = Expression.Lambda(Expression.Call(Expression.Call(Expression.Constant(response), getAwaiterMethod),
                "GetResult", null)).Compile();

            getResult.DynamicInvoke();

            await _next(context);
        }

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
    }
}