using System;

namespace Rabbit.Cloud
{
    public interface IServiceNameProvider
    {
        string ServiceName { get; }
    }

    public interface IServiceMethodProvider : IServiceNameProvider
    {
        string FullName { get; }
        string MethodName { get; }
        Type RequestType { get; }
        Type ResponseType { get; }
    }

    public interface IServiceDefinitionProvider : IServiceNameProvider { }

    public interface IClientDefinitionProvider : IServiceNameProvider
    {
        string Host { get; }
        string Protocol { get; }
    }

    public interface IServiceIgnoreProvider { }

    public class RabbitServiceAttribute : Attribute, IServiceDefinitionProvider
    {
        public RabbitServiceAttribute(string serviceName)
        {
            ServiceName = serviceName;
        }

        public RabbitServiceAttribute()
        {
        }

        #region Implementation of IServiceNameProvider

        public string ServiceName { get; set; }

        #endregion Implementation of IServiceNameProvider
    }

    public class RabbitClientAttribute : Attribute, IClientDefinitionProvider
    {
        public RabbitClientAttribute(string url)
        {
            if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
                throw new ArgumentException("illegal url.", nameof(url));

            Protocol = uri.Scheme;
            Host = uri.IsDefaultPort ? uri.Host : $"{uri.Host}:{uri.Port}";
            ServiceName = uri.AbsolutePath.TrimStart('/');
        }

        public RabbitClientAttribute()
        {
        }

        #region Implementation of IServiceNameProvider

        public string Host { get; set; }
        public string Protocol { get; set; }
        public string ServiceName { get; set; }

        #endregion Implementation of IServiceNameProvider
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class RabbitServiceMethodAttribute : Attribute, IServiceMethodProvider
    {
        public RabbitServiceMethodAttribute()
        {
        }

        public RabbitServiceMethodAttribute(string methodName)
        {
            MethodName = methodName;
        }

        public RabbitServiceMethodAttribute(string serviceName, string methodName) : this(methodName)
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

    [AttributeUsage(AttributeTargets.Method)]
    public class NonServiceMethodAttribute : Attribute, IServiceIgnoreProvider { }
}