using Grpc.Core;
using Rabbit.Cloud.Application.Abstractions;
using Rabbit.Cloud.Client.Abstractions;
using Rabbit.Cloud.Client.Abstractions.Features;
using Rabbit.Cloud.Client.Grpc.Features;
using Rabbit.Cloud.Grpc.Utilities;
using Rabbit.Cloud.Grpc.Utilities.Extensions;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Client.Grpc
{
    public class GrpcMiddleware
    {
        private readonly RabbitRequestDelegate _next;

        public GrpcMiddleware(RabbitRequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(IRabbitContext context)
        {
            var request = context.Request;
            var response = context.Response;
            var serviceRequestFeature = context.Features.Get<IServiceRequestFeature>();
            var grpcFeature = context.Features.Get<IGrpcFeature>();

            var serviceInstance = serviceRequestFeature.GetServiceInstance();

            var method = request.Path;

            try
            {
                var channel = grpcFeature.Channel;
                var callOptions = grpcFeature.CallOptions ?? new CallOptions();

                var t = CallInvokerExtensions.Call(serviceRequestFeature.RequesType,
                    serviceRequestFeature.ResponseType,
                    request.Body,
                    channel,
                    method,
                    serviceInstance.Host,
                    grpcFeature.RequestMarshaller,
                    grpcFeature.ResponseMarshaller,
                    callOptions);

                var grpcResponse = t;
                //                var grpcResponse = callInvoker.Call(method, serviceInstance.Host, callOptions, request.Body);

                //                todo: await result, may trigger exception.
                var task = FluentUtilities.WrapperCallResuleToTask(grpcResponse);
                await task;

                //                response.Body = task.GetType().GetProperty("Result")?.GetValue(task);
                response.Body = task;
            }
            catch (RpcException rpcException)
            {
                ExceptionUtilities.ServiceRequestFailure($"{serviceInstance.Host}:{serviceInstance.Port}/{method}", RpcExceptionExtensions.GetStatusCode(rpcException.Status.StatusCode), rpcException);
            }

            await _next(context);
        }
    }
}