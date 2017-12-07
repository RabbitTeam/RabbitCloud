using Grpc.Core;

namespace Rabbit.Cloud.Grpc.Abstractions.Client
{
    public interface ICallInvokerFactory
    {
        CallInvoker GetCallInvoker(string host, int port);
    }
}