namespace Rabbit.Cloud.Grpc.Client
{
    public interface IMethodProvider
    {
        void Collect(IMethodCollection methods);
    }
}