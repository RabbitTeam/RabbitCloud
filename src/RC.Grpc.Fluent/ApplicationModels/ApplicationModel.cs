using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Rabbit.Cloud.Grpc.Fluent.ApplicationModels
{
    public class MarshallerModel
    {
        public MarshallerModel(TypeInfo type, IReadOnlyList<object> attributes)
        {
            if (attributes == null)
                throw new ArgumentNullException(nameof(attributes));

            Type = type ?? throw new ArgumentNullException(nameof(type));
            Attributes = new List<object>(attributes);
        }

        public MethodModel MethodModel { get; set; }

        /// <summary>
        /// Target Type.
        /// </summary>
        public TypeInfo Type { get; }

        public IReadOnlyList<object> Attributes { get; }

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
        public MethodModel(MethodInfo methodInfo, IReadOnlyList<object> attributes)
        {
            if (attributes == null)
                throw new ArgumentNullException(nameof(attributes));

            MethodInfo = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));
            Attributes = new List<object>(attributes);
        }

        public IReadOnlyList<object> Attributes { get; }
        public MethodInfo MethodInfo { get; }
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
        public ServiceModel(TypeInfo serviceType, IReadOnlyList<object> attributes)
        {
            if (attributes == null)
                throw new ArgumentNullException(nameof(attributes));

            ServiceType = serviceType ?? throw new ArgumentNullException(nameof(serviceType));
            Attributes = new List<object>(attributes);
        }

        public TypeInfo ServiceType { get; }
        public string ServiceName { get; set; }
        public IReadOnlyList<object> Attributes { get; }

        public IList<MethodModel> Methods { get; } = new List<MethodModel>();
    }

    public class ServerMethodModel
    {
        public ServerMethodModel(MethodInfo methodInfo, IReadOnlyList<object> attributes)
        {
            if (attributes == null)
                throw new ArgumentNullException(nameof(attributes));

            MethodInfo = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));
            Attributes = new List<object>(attributes);
        }

        public IReadOnlyList<object> Attributes { get; }
        public ServerServiceModel ServerService { get; set; }
        public MethodModel Method { get; set; }
        public MethodInfo MethodInfo { get; }
    }

    public class ServerServiceModel
    {
        public ServerServiceModel(TypeInfo type, IReadOnlyList<object> attributes)
        {
            if (attributes == null)
                throw new ArgumentNullException(nameof(attributes));

            Type = type ?? throw new ArgumentNullException(nameof(type));
            Attributes = new List<object>(attributes);
        }

        public IReadOnlyList<object> Attributes { get; }
        public string ServiceName { get; set; }
        public TypeInfo Type { get; }
        public IList<ServerMethodModel> ServerMethods { get; } = new List<ServerMethodModel>();
    }

    public class ApplicationModel
    {
        public IList<ServiceModel> Services { get; } = new List<ServiceModel>();
        public IList<ServerServiceModel> ServerServices { get; } = new List<ServerServiceModel>();
    }
}