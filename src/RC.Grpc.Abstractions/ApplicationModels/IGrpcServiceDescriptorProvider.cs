namespace Rabbit.Cloud.Grpc.Abstractions.ApplicationModels
{
    public interface IGrpcServiceDescriptorProvider
    {
        void Collect(IGrpcServiceDescriptorCollection serviceDescriptors);
    }
}