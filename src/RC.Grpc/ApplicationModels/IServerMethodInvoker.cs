using Grpc.Core;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Grpc.ApplicationModels
{
    public interface IServerMethodInvoker
    {
        Task<TResponse> UnaryServerMethod<TRequest, TResponse>(TRequest request, ServerCallContext callContext);

        Task<TResponse> ClientStreamingServerMethod<TRequest, TResponse>(IAsyncStreamReader<TRequest> requestStream, ServerCallContext callContext);

        Task ServerStreamingServerMethod<TRequest, TResponse>(TRequest request, IServerStreamWriter<TResponse> responseStream, ServerCallContext callContext);

        Task DuplexStreamingServerMethod<TRequest, TResponse>(IAsyncStreamReader<TRequest> requestStream, IServerStreamWriter<TResponse> responseStream, ServerCallContext callContext);
    }
}