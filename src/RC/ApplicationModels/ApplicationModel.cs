using System;
using System.Collections.Generic;
using System.Reflection;

namespace Rabbit.Cloud.ApplicationModels
{
    public class CodecModel
    {
        public CodecModel(TypeInfo type, IReadOnlyList<object> attributes)
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
        /// Gets the unqualified name of the method.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the codec used for request messages.
        /// </summary>
        public CodecModel RequestCodec { get; set; }

        /// <summary>
        /// Gets the codec used for response messages.
        /// </summary>
        public CodecModel ResponseCodec { get; set; }
    }

    public class ServiceModel
    {
        public ServiceModel(TypeInfo serviceType, IReadOnlyList<object> attributes)
        {
            if (attributes == null)
                throw new ArgumentNullException(nameof(attributes));

            Type = serviceType ?? throw new ArgumentNullException(nameof(serviceType));
            Attributes = new List<object>(attributes);
        }

        public TypeInfo Type { get; }
        public string ServiceName { get; set; }
        public IReadOnlyList<object> Attributes { get; }

        public IList<MethodModel> Methods { get; } = new List<MethodModel>();
    }

    public class ApplicationModel
    {
        public IList<ServiceModel> Services { get; } = new List<ServiceModel>();
    }
}