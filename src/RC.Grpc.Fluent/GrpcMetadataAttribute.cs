using System;

namespace Rabbit.Cloud.Grpc.Fluent
{
    public interface IGrpcServiceNameProvider
    {
        string ServiceName { get; }
    }

    public interface IGrpcMethodProvider : IGrpcServiceNameProvider
    {
        string FullName { get; }
        string MethodName { get; }
        Type RequestType { get; }
        Type ResponseType { get; }
    }

    public interface IGrpcDefinitionProvider : IGrpcServiceNameProvider { }

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

        #region Implementation of IGrpcMethodProvider

        public string FullName { get; set; }
        public string MethodName { get; set; }
        public Type RequestType { get; set; }
        public Type ResponseType { get; set; }

        #endregion Implementation of IGrpcMethodProvider
    }

    public interface IGrpcIgnoreProvider { }

    [AttributeUsage(AttributeTargets.Method)]
    public class NonGrpcMethodAttribute : Attribute, IGrpcIgnoreProvider { }
}