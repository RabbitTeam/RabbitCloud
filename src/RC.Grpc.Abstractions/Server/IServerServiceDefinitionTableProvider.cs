namespace Rabbit.Cloud.Grpc.Abstractions.Server
{
    public interface IServerServiceDefinitionTableProvider
    {
        IServerServiceDefinitionTable ServerServiceDefinitionTable { get; }
    }
}