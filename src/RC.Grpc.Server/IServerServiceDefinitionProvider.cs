namespace Rabbit.Cloud.Grpc.Server
{
    public interface IServerServiceDefinitionProvider
    {
        void Collect(IServerMethodCollection serverDelegates);
    }
}