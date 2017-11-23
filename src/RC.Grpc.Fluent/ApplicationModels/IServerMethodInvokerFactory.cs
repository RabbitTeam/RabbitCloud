namespace Rabbit.Cloud.Grpc.Fluent.ApplicationModels
{
    public interface IServerMethodInvokerFactory
    {
        IServerMethodInvoker CreateInvoker(ServerMethodModel serverMethod);
    }
}