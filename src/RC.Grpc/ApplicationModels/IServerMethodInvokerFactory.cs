using Rabbit.Cloud.ApplicationModels;

namespace Rabbit.Cloud.Grpc.ApplicationModels
{
    public interface IServerMethodInvokerFactory
    {
        IServerMethodInvoker CreateInvoker(MethodModel serverMethod);
    }
}