using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Grpc.Fluent.ApplicationModels.Internal
{
    public class DefaultServerMethodInvoker : ServerMethodInvoker
    {
        private readonly ILogger<DefaultServerMethodInvoker> _logger;

        public DefaultServerMethodInvoker(ServerMethodModel serverMethod, IServiceProvider services, ILogger<DefaultServerMethodInvoker> logger) : base(serverMethod, services, logger)
        {
            _logger = logger;
        }

        #region Overrides of ServerMethodInvoker<TRequest,TResponse>

        public override Task<TResponse> UnaryServerMethod<TRequest, TResponse>(TRequest request, ServerCallContext callContext)
        {
            try
            {
                return (Task<TResponse>)MethodInvoker(new object[] { request, callContext });
            }
            catch (Exception e)
            {
                callContext.RequestHeaders.Add("exception", e.Message);
                _logger.LogError(e, $"invoke method {ServerMethod.MethodInfo.DeclaringType.FullName}.{ServerMethod.MethodInfo.Name} error.");
                throw;
            }
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