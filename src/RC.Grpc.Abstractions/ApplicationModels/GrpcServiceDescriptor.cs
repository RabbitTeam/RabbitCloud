using Rabbit.Cloud.Abstractions;
using Rabbit.Cloud.Grpc.Abstractions.Adapter;
using Rabbit.Cloud.Grpc.Abstractions.Utilities;
using System;
using System.Reflection;

namespace Rabbit.Cloud.Grpc.Abstractions.ApplicationModels
{
    public interface IGrpcServiceDescriptorCollection : IVersionCollection<string, GrpcServiceDescriptor>
    {
    }

    public class GrpcServiceDescriptorCollection : VersionCollection<string, GrpcServiceDescriptor>, IGrpcServiceDescriptorCollection
    {
    }

    public class GrpcServiceDescriptor
    {
        public string ServiceId { get; set; }
        public Marshaller RequestMarshaller { get; set; }
        public Marshaller ResponseMarshaller { get; set; }

        public static GrpcServiceDescriptor Create(Type serviceType, MethodInfo methodInfo)
        {
            (string serviceName, string methodName) = ReflectionUtilities.GetServiceNames(serviceType, methodInfo);

            return new GrpcServiceDescriptor
            {
                ServiceId = $"/{serviceName}/{methodName}"
            };
        }
    }
}