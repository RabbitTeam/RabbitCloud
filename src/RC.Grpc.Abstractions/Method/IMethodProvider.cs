namespace Rabbit.Cloud.Grpc.Abstractions.Method
{
    public interface IMethodProvider
    {
        void Collect(IMethodCollection methods);
    }
}