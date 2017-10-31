using Rabbit.Cloud.Client.Abstractions;
using Rabbit.Cloud.Client.Features;
using Rabbit.Cloud.Client.Grpc.Features;
using Rabbit.Cloud.Client.Grpc.Internal;
using Rabbit.Cloud.Grpc.Client;
using Rabbit.Cloud.Grpc.Client.Extensions;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Client.Grpc
{
    public class GrpcMiddleware
    {
        private readonly RabbitRequestDelegate<GrpcRabbitContext> _next;
        private readonly CallInvokerPool _callInvokerPool;
        private readonly IMethodCollection _methodCollection;

        public GrpcMiddleware(RabbitRequestDelegate<GrpcRabbitContext> next, CallInvokerPool callInvokerPool, IMethodCollection methodCollection)
        {
            _next = next;
            _callInvokerPool = callInvokerPool;
            _methodCollection = methodCollection;
        }

        public async Task Invoke(GrpcRabbitContext context)
        {
            var grpcRequestFeature = context.Features.Get<IGrpcRequestFeature>();

            var requestFeature = context.Features.Get<IRequestFeature>();
            var serviceUrl = requestFeature.ServiceUrl;

            var callInvoker = _callInvokerPool.GetCallInvoker(serviceUrl.Host, serviceUrl.Port);

            var serviceId = serviceUrl.Path;
            var method = _methodCollection.Get(serviceId);

            var response = callInvoker.Call(method, grpcRequestFeature.Host, grpcRequestFeature.CallOptions, grpcRequestFeature.Request);

            context.Features.Set<IGrpcResponseFeature>(new GrpcResponseFeature { Response = response });

            await _next(context);
        }
    }
}