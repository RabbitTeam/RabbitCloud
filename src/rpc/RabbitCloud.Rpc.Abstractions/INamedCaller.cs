namespace RabbitCloud.Rpc.Abstractions
{
    public interface INamedCaller : ICaller
    {
        string Name { get; }
    }
}