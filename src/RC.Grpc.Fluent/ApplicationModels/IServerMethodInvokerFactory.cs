namespace Rabbit.Cloud.Grpc.Fluent.ApplicationModels
{
    public interface IServerMethodInvokerFactory
    {
        ServerMethodInvoker CreateInvoker(ServerMethodModel serverMethod);
    }
}