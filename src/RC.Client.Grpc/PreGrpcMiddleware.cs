using Grpc.Core;
using Microsoft.Extensions.Primitives;
using Rabbit.Cloud.Application.Abstractions;
using Rabbit.Cloud.Client.Abstractions.Features;
using Rabbit.Cloud.Client.Grpc.Features;
using Rabbit.Cloud.Grpc.Abstractions;
using Rabbit.Cloud.Grpc.Abstractions.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Client.Grpc
{
    public class PreGrpcMiddleware
    {
        private readonly RabbitRequestDelegate _next;
        private readonly ICallInvokerFactory _callInvokerFactory;
        private readonly IMethodTable _methodTable;

        public PreGrpcMiddleware(RabbitRequestDelegate next, ICallInvokerFactory callInvokerFactory, IMethodTableProvider methodTableProvider)
        {
            _next = next;
            _callInvokerFactory = callInvokerFactory;
            _methodTable = methodTableProvider.MethodTable;
        }

        public async Task Invoke(IRabbitContext context)
        {
            var request = context.Request;
            var serviceRequestFeature = context.Features.Get<IServiceRequestFeature>();

            var grpcFeature = context.Features.Get<IGrpcFeature>();
            if (grpcFeature == null)
            {
                grpcFeature = new GrpcFeature();
                context.Features.Set(grpcFeature);
            }

            var serviceInstance = serviceRequestFeature.ServiceInstance;
            var requestOptions = serviceRequestFeature.RequestOptions;

            if (grpcFeature.Method == null)
            {
                var serviceName = request.Path;
                var method = _methodTable.Get(serviceName);
                grpcFeature.Method = method;
            }
            if (grpcFeature.CallInvoker == null)
            {
                var callInvoker = await _callInvokerFactory.GetCallInvokerAsync(serviceInstance.Host, serviceInstance.Port, requestOptions.ConnectionTimeout);
                grpcFeature.CallInvoker = callInvoker;
            }

            var callOptionsNullable = grpcFeature.CallOptions;

            CallOptions callOptions;

            //追加选项
            if (callOptionsNullable.HasValue)
            {
                callOptions = callOptionsNullable.Value;

                if (!callOptions.Deadline.HasValue)
                    callOptions = callOptions.WithDeadline(DateTime.UtcNow.Add(requestOptions.ReadTimeout));

                var headers = callOptions.Headers ?? new Metadata();
                if (AppendHeaders(headers, request))
                    callOptions = callOptions.WithHeaders(headers);
            }
            else
            {
                var headers = new Metadata();
                if (!AppendHeaders(headers, request))
                    headers = null;
                callOptions = new CallOptions(headers, DateTime.UtcNow.Add(requestOptions.ReadTimeout));
            }

            grpcFeature.CallOptions = callOptions;

            await _next(context);
        }

        private static bool AppendHeaders(Metadata headers, IRabbitRequest request)
        {
            IDictionary<string, StringValues> appendHeaders = null;

            if (request.Query.Any() || request.Headers.Any())
            {
                appendHeaders = new Dictionary<string, StringValues>();
                foreach (var query in request.Query)
                {
                    StringValues values;
                    values = appendHeaders.TryGetValue(query.Key, out values) ? StringValues.Concat(values, query.Value) : query.Value;
                    appendHeaders[query.Key] = values;
                }
                foreach (var header in request.Headers)
                {
                    StringValues values;
                    values = appendHeaders.TryGetValue(header.Key, out values) ? StringValues.Concat(values, header.Value) : header.Value;
                    appendHeaders[header.Key] = values;
                }
            }

            if (appendHeaders == null || appendHeaders.Any())
                return false;

            foreach (var header in appendHeaders)
            {
                headers.Add(header.Key, header.Value.ToString());
            }
            return true;
        }
    }
}