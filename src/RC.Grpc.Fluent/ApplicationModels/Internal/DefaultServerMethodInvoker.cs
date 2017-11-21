using Grpc.Core;
using System;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Grpc.Fluent.ApplicationModels.Internal
{
    public class DefaultServerMethodInvoker : ServerMethodInvoker
    {
        public DefaultServerMethodInvoker(ServerMethodModel serverMethod, IServiceProvider services) : base(serverMethod, services)
        {
        }

        #region Overrides of ServerMethodInvoker<TRequest,TResponse>

        public override Task<TResponse> UnaryServerMethod<TRequest, TResponse>(TRequest request, ServerCallContext callContext)
        {
            return (Task<TResponse>)MethodInvoker(new object[] { request, callContext });
        }

        public override Task<TResponse> ClientStreamingServerMethod<TRequest, TResponse>(IAsyncStreamReader<TRequest> requestStream, ServerCallContext callContext)
        {
            throw new NotImplementedException();
        }

        public override Task ServerStreamingServerMethod<TRequest, TResponse>(TRequest request, IServerStreamWriter<TResponse> responseStream, ServerCallContext callContext)
        {
            throw new NotImplementedException();
        }

        public override Task DuplexStreamingServerMethod<TRequest, TResponse>(IAsyncStreamReader<TRequest> requestStream, IServerStreamWriter<TResponse> responseStream,
            ServerCallContext callContext)
        {
            throw new NotImplementedException();
        }

        #endregion Overrides of ServerMethodInvoker<TRequest,TResponse>
    }
}