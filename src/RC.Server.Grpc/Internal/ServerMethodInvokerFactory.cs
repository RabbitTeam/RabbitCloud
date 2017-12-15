using Grpc.Core;
using Microsoft.Extensions.Logging;
using Rabbit.Cloud.Application.Abstractions;
using Rabbit.Cloud.ApplicationModels;
using Rabbit.Cloud.Grpc.ApplicationModels;
using Rabbit.Cloud.Grpc.ApplicationModels.Internal;
using System;
using System.Threading.Tasks;
using Rabbit.Cloud.Application;
using Rabbit.Cloud.Application.Features;
using Rabbit.Cloud.Server.Grpc.Features;

namespace Rabbit.Cloud.Server.Grpc.Internal
{
    public class ServerMethodInvokerFactory : IServerMethodInvokerFactory
    {
        private readonly RabbitRequestDelegate _invoker;
        private readonly DefaultServerMethodInvokerFactory _defaultServerMethodInvokerFactory;

        public ServerMethodInvokerFactory(RabbitRequestDelegate invoker, IServiceProvider services, ILogger<DefaultServerMethodInvoker> logger)
        {
            _invoker = invoker;
            _defaultServerMethodInvokerFactory = new DefaultServerMethodInvokerFactory(services, logger);
        }

        #region Implementation of IServerMethodInvokerFactory

        public IServerMethodInvoker CreateInvoker(MethodModel serverMethod)
        {
            var serverMethodInvoker = _defaultServerMethodInvokerFactory.CreateInvoker(serverMethod);
            return new GrpcServerMethodInvoker(serverMethodInvoker, _invoker);
        }

        #endregion Implementation of IServerMethodInvokerFactory

        private class GrpcServerMethodInvoker : IServerMethodInvoker
        {
            private readonly IServerMethodInvoker _serverMethodInvoker;
            private readonly RabbitRequestDelegate _invoker;

            public GrpcServerMethodInvoker(IServerMethodInvoker serverMethodInvoker, RabbitRequestDelegate invoker)
            {
                _serverMethodInvoker = serverMethodInvoker;
                _invoker = invoker;
            }

            #region Implementation of IServerMethodInvoker

            public async Task<TResponse> UnaryServerMethod<TRequest, TResponse>(TRequest request, ServerCallContext callContext)
            {
                var context = new RabbitContext();
                context.Request.Request = request;

                var grpcServerFeature = context.Features.GetOrAdd<IGrpcServerFeature>(()=>new GrpcServerFeature());

                grpcServerFeature.ServerCallContext = callContext;

                grpcServerFeature.ResponseInvoker = async () =>
                {
                    if (context.Response.Response != null)
                        return context.Response.Response;
                    return context.Response.Response =
                        await _serverMethodInvoker.UnaryServerMethod<TRequest, TResponse>(request, callContext);
                };

                await _invoker(context);

                grpcServerFeature.ResponseType = typeof(TResponse);
                return (TResponse)context.Response.Response;
            }

            public Task<TResponse> ClientStreamingServerMethod<TRequest, TResponse>(IAsyncStreamReader<TRequest> requestStream, ServerCallContext callContext)
            {
                throw new NotImplementedException();
            }

            public Task ServerStreamingServerMethod<TRequest, TResponse>(TRequest request, IServerStreamWriter<TResponse> responseStream,
                ServerCallContext callContext)
            {
                throw new NotImplementedException();
            }

            public Task DuplexStreamingServerMethod<TRequest, TResponse>(IAsyncStreamReader<TRequest> requestStream,
                IServerStreamWriter<TResponse> responseStream, ServerCallContext callContext)
            {
                throw new NotImplementedException();
            }

            #endregion Implementation of IServerMethodInvoker
        }
    }
}