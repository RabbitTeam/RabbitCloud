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

            if (grpcRequestFeature == null)
                throw new ArgumentNullException(nameof(grpcRequestFeature));

            grpcRequestFeature.CallOptions = grpcRequestFeature.CallOptions.WithDeadline(DateTime.UtcNow.AddSeconds(5));

            var requestFeature = context.Features.Get<IRequestFeature>();
            var serviceUrl = requestFeature.ServiceUrl;

            if (serviceUrl == null)
                throw new ArgumentNullException(nameof(requestFeature.ServiceUrl));

            var callInvoker = _callInvokerPool.GetCallInvoker(serviceUrl.Host, serviceUrl.Port);

            var serviceId = serviceUrl.Path;
            var method = _methodCollection.Get(serviceId);

            if (method == null)
                throw new Exception($"Can not find service '{serviceId}'.");

            var response = callInvoker.Call(method, grpcRequestFeature.Host, grpcRequestFeature.CallOptions, grpcRequestFeature.Request);

            context.Features.Get<IGrpcResponseFeature>().Response = response;

            var awaiterDelegate = Cache.GetAwaiterDelegate(response.GetType());
            awaiterDelegate.DynamicInvoke(response);

            await _next(context);
        }

        #region Help Type

        private static class Cache
        {
            private static readonly IDictionary<Type, Delegate> Caches = new Dictionary<Type, Delegate>();

            public static Delegate GetAwaiterDelegate(Type type)
            {
                if (Caches.TryGetValue(type, out var action))
                    return action;

                var getAwaiterMethod = type.GetMethod(nameof(Task.GetAwaiter));
                var parameterExpression = Expression.Parameter(type);
                var callExpression = Expression.Call(Expression.Call(parameterExpression, getAwaiterMethod), nameof(TaskAwaiter.GetResult), null);

                return Caches[type] = Expression.Lambda(callExpression, parameterExpression).Compile();
            }
        }

        #endregion Help Type
    }
}