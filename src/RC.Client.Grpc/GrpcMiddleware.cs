using Rabbit.Cloud.Client.Abstractions;
using Rabbit.Cloud.Client.Features;
using Rabbit.Cloud.Client.Grpc.Features;
using Rabbit.Cloud.Grpc.Abstractions.Method;
using Rabbit.Cloud.Grpc.Client.Extensions;
using Rabbit.Cloud.Grpc.Client.Internal;
using System;
using System.Linq.Expressions;
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

            context.Features.Get<IGrpcResponseFeature>().Response = response;

            var getAwaiterMethod = response.GetType().GetMethod("GetAwaiter");

            if (getAwaiterMethod != null)
            {
                var getResult = Expression.Lambda(Expression.Call(Expression.Call(Expression.Constant(response), getAwaiterMethod),
                    "GetResult", null)).Compile();

                getResult.DynamicInvoke();
            }

            await _next(context);
        }
    }
}