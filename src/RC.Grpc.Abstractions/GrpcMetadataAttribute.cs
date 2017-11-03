using System;

namespace Rabbit.Cloud.Grpc.Abstractions
{
    public interface IGrpcServiceNameProvider
    {
        string ServiceName { get; }
    }

    public interface IGrpcDefinitionProvider : IGrpcServiceNameProvider { }

    public interface IGrpcMethodProvider : IGrpcServiceNameProvider
    {
        string FullName { get; }
        string MethodName { get; }
        Type RequestType { get; }
        Type ResponseType { get; }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Method)]
    public class GrpcServiceAttribute : Attribute, IGrpcDefinitionProvider
    {
        public GrpcServiceAttribute()
        {
        }

        public GrpcServiceAttribute(string serviceName)
        {
            ServiceName = serviceName;
        }

        #region Implementation of IGrpcServiceNameProvider

        public string ServiceName { get; }

        #endregion Implementation of IGrpcServiceNameProvider
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Method)]
    public class GrpcClientAttribute : Attribute, IGrpcDefinitionProvider
    {
        public GrpcClientAttribute()
        {
        }

        public GrpcClientAttribute(string serviceName)
        {
            ServiceName = serviceName;
        }

        #region Implementation of IGrpcServiceNameProvider

        public string ServiceName { get; }

        #endregion Implementation of IGrpcServiceNameProvider
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class GrpcMethodAttribute : Attribute, IGrpcMethodProvider
    {
        public GrpcMethodAttribute()
        {
        }

        public GrpcMethodAttribute(string methodName)
        {
            MethodName = methodName;
        }

        public GrpcMethodAttribute(string serviceName, string methodName) : this(methodName)
        {
            ServiceName = serviceName;
        }

        #region Implementation of IGrpcServiceNameProvider

        public string ServiceName { get; set; }

        #endregion Implementation of IGrpcServiceNameProvider

        #region Implementation of IGrpcMethodNameProvider

        public string MethodName { get; set; }

        #endregion Implementation of IGrpcMethodNameProvider

        #region Implementation of IGrpcFullNameProvider

        public string FullName { get; set; }

        #endregion Implementation of IGrpcFullNameProvider

        #region Implementation of IGrpcMethodProvider

        public Type RequestType { get; set; }
        public Type ResponseType { get; set; }

        #endregion Implementation of IGrpcMethodProvider
    }
}