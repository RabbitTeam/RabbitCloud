using Rabbit.Cloud.Abstractions;

namespace Rabbit.Cloud.Grpc.Abstractions
{
    public class GrpcOptions
    {
        public IVersionCollection<string, GrpcServiceDescriptor> ServiceDescriptors { get; } = new VersionCollection<string, GrpcServiceDescriptor>();
    }
}