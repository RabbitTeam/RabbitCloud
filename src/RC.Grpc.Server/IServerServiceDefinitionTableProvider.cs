namespace Rabbit.Cloud.Grpc.Server
{
    public interface IServerServiceDefinitionTableProvider
    {
        IServerServiceDefinitionTable ServerServiceDefinitionTable { get; }
    }
}