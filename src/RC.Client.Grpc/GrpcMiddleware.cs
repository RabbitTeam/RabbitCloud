using Grpc.Core;
using Rabbit.Cloud.Abstractions;
using Rabbit.Cloud.Application.Abstractions;
using Rabbit.Cloud.Client.Abstractions.Features;
using Rabbit.Cloud.Grpc.Abstractions;
using Rabbit.Cloud.Grpc.Abstractions.Client;
using Rabbit.Cloud.Grpc.Abstractions.Utilities.Extensions;
using Rabbit.Cloud.Grpc.Utilities;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Client.Grpc
{
    public class GrpcMiddleware
    {
        private readonly RabbitRequestDelegate _next;
        private readonly ICallInvokerFactory _callInvokerFactory;
        private readonly IMethodTable _methodTable;

        public GrpcMiddleware(RabbitRequestDelegate next, ICallInvokerFactory callInvokerFactory, IMethodTableProvider methodTableProvider)
        {
            _next = next;
            _callInvokerFactory = callInvokerFactory;
            _methodTable = methodTableProvider.MethodTable;
        }

        public async Task Invoke(IRabbitContext context)
        {
            var request = context.Request;
            var response = context.Response;
            var serviceRequestFeature = context.Features.Get<IServiceRequestFeature>();

            var serviceInstance = serviceRequestFeature.ServiceInstance;
            var requestOptions = serviceRequestFeature.RequestOptions;

            var serviceName = request.Path;
            var method = _methodTable.Get(serviceName);

            if (method == null)
                throw new RabbitRpcException(RabbitRpcExceptionCode.Forbidden, $"Can not find service '{serviceName}'.");

            var callInvoker = await _callInvokerFactory.GetCallInvokerAsync(serviceInstance.Host, serviceInstance.Port, requestOptions.ConnectionTimeout);

            var headers = new Metadata();
            foreach (var header in request.Query.Concat(request.Headers))
                headers.Add(header.Key, header.Value.ToString());

            var callOptions = new CallOptions(headers, DateTime.UtcNow.Add(requestOptions.ReadTimeout));

            try
            {
                var grpcResponse = callInvoker.Call(method, serviceInstance.Host, callOptions, request.Body);

                //todo: await result, may trigger exception.
                var task = FluentUtilities.WrapperCallResuleToTask(grpcResponse);
                await task;

                response.Body = task.GetType().GetProperty("Result")?.GetValue(task);
            }
            catch (RpcException rpcException)
            {
                throw rpcException.WrapRabbitRpcException();
            }

            await _next(context);
        }
    }
}