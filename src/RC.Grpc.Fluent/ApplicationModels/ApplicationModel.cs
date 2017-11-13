using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Rabbit.Cloud.Grpc.Fluent.ApplicationModels
{
    public class MarshallerModel
    {
        public MethodModel MethodModel { get; set; }

        /// <summary>
        /// Target Type.
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// Gets the serializer function.
        /// </summary>
        public Func<object, byte[]> Serializer { get; set; }

        /// <summary>
        /// Gets the deserializer function.
        /// </summary>
        public Func<byte[], object> Deserializer { get; set; }
    }

    public class MethodModel
    {
        public MethodInfo MethodInfo { get; set; }
        public ServiceModel ServiceModel { get; set; }

        /// <summary>
        /// Gets the type of the method.
        /// </summary>
        public MethodType Type { get; set; }

        /// <summary>
        /// Gets the unqualified name of the method.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the marshaller used for request messages.
        /// </summary>
        public MarshallerModel RequestMarshaller { get; set; }

        /// <summary>
        /// Gets the marshaller used for response messages.
        /// </summary>
        public MarshallerModel ResponseMarshaller { get; set; }
    }

    public class ServiceModel
    {
        public TypeInfo ServiceType { get; set; }
        public string ServiceName { get; set; }

        public ICollection<MethodModel> Methods { get; } = new List<MethodModel>();
    }

    public class ServerMethodModel
    {
        public ServerServiceModel ServerService { get; set; }
        public MethodModel Method { get; set; }
        public MethodInfo MethodInfo { get; set; }
    }

    public class ServerServiceModel
    {
        public string ServiceName { get; set; }
        public Type Type { get; set; }
        public ICollection<ServerMethodModel> ServerMethods { get; } = new List<ServerMethodModel>();
    }

    public class ApplicationModel
    {
        public ICollection<ServiceModel> Services { get; } = new List<ServiceModel>();
        public ICollection<ServerServiceModel> ServerServices { get; } = new List<ServerServiceModel>();
    }
}