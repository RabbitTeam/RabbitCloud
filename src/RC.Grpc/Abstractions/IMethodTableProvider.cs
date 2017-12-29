namespace Rabbit.Cloud.Grpc.Abstractions
{
    public interface IMethodTableProvider
    {
        IMethodTable MethodTable { get; }
    }
}