using Rabbit.Cloud.Application.Abstractions;
using Rabbit.Cloud.Application.Features;
using Rabbit.Cloud.Client.Grpc.Features;
using Rabbit.Cloud.Grpc.Abstractions;
using Rabbit.Cloud.Grpc.Client.Extensions;
using Rabbit.Cloud.Grpc.Client.Internal;
using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Client.Grpc
{
    public class GrpcMiddleware
    {
        private readonly RabbitRequestDelegate _next;
        private readonly CallInvokerPool _callInvokerPool;
        private readonly IMethodTable _methodTable;

        public GrpcMiddleware(RabbitRequestDelegate next, CallInvokerPool callInvokerPool, IMethodTableProvider methodTableProvider)
        {
            _next = next;
            _callInvokerPool = callInvokerPool;
            _methodTable = methodTableProvider.MethodTable;
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
            var method = _methodTable.Get(serviceId);

            if (method == null)
                throw new Exception($"Can not find service '{serviceId}'.");

            var response = callInvoker.Call(method, grpcRequestFeature.Host, grpcRequestFeature.CallOptions, grpcRequestFeature.Request);

            context.Features.Get<IGrpcResponseFeature>().Response = response;

            //todo: await result, may trigger exception.
            var awaiterAction = Cache.GetAwaiterAction(response.GetType());
            awaiterAction(response);

            await _next(context);
        }

        #region Help Type

        private static class Cache
        {
            private static readonly ConcurrentDictionary<Type, Lazy<Action<object>>> Caches = new ConcurrentDictionary<Type, Lazy<Action<object>>>();

            public static Action<object> GetAwaiterAction(Type type)
            {
                var item = Caches.GetOrAdd(type, k =>
                  {
                      return new Lazy<Action<object>>(() =>
                      {
                          var getAwaiterMethod = type.GetMethod(nameof(Task.GetAwaiter));
                          var parameterExpression = Expression.Parameter(typeof(object));
                          var callExpression =
                              Expression.Call(
                                  Expression.Call(Expression.Convert(parameterExpression, type), getAwaiterMethod),
                                  nameof(TaskAwaiter.GetResult), null);
                          return Expression.Lambda<Action<object>>(callExpression, parameterExpression).Compile();
                      });
                  });

                return item.Value;
            }
        }

        #endregion Help Type
    }
}