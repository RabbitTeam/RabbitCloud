using Microsoft.Extensions.Primitives;
using Rabbit.Cloud.Client.Abstractions.Utilities;
using System;
using System.Collections.Generic;

namespace Rabbit.Cloud.Client.Abstractions
{
    public interface IRequestHeaderProvider
    {
        IDictionary<string, StringValues> Headers { get; }
    }

    public interface IRequestParameterProvider
    {
        IDictionary<string, StringValues> Parameters { get; }
    }

    [AttributeUsage(AttributeTargets.Interface)]
    public class RabbitClientAttribute : Attribute
    {
        public string BaseUrl { get; set; }
    }

    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method)]
    public class RequestMappingAttribute : Attribute, IRequestParameterProvider
    {
        public RequestMappingAttribute(string value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var result = ClientUtilities.SplitPathAndQuery(value);
            Path = result.Path;
            Parameters = result.Query;
        }

        public RequestMappingAttribute(string path, IDictionary<string, StringValues> parameters = null)
        {
            Path = string.IsNullOrEmpty(path) ? throw new ArgumentNullException(nameof(path)) : path;
            Parameters = parameters;
        }

        public string Path { get; set; }

        #region Implementation of IRequestParameterProvider

        public IDictionary<string, StringValues> Parameters { get; }

        #endregion Implementation of IRequestParameterProvider
    }

    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method)]
    public class DefaultRequestHeaderAttribute : Attribute, IRequestHeaderProvider
    {
        public DefaultRequestHeaderAttribute(string name, StringValues? value = null)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            Headers = new Dictionary<string, StringValues>(1)
            {
                {name,value??StringValues.Empty }
            };
        }

        public DefaultRequestHeaderAttribute(IDictionary<string, StringValues> headers)
        {
            Headers = headers ?? throw new ArgumentNullException(nameof(headers));
        }

        public DefaultRequestHeaderAttribute(string query)
        {
            if (string.IsNullOrEmpty(query))
                throw new ArgumentException(nameof(query));

            Headers = ClientUtilities.ParseNullableQuery(query);
        }

        #region Implementation of IRequestHeaderProvider

        public IDictionary<string, StringValues> Headers { get; }

        #endregion Implementation of IRequestHeaderProvider
    }

    [AttributeUsage(AttributeTargets.Interface | AttributeTargets.Method)]
    public class DefaultRequestParameterAttribute : Attribute, IRequestParameterProvider
    {
        public DefaultRequestParameterAttribute(string name, StringValues? value = null)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            Parameters = new Dictionary<string, StringValues>(1)
            {
                {name,value??StringValues.Empty }
            };
        }

        public DefaultRequestParameterAttribute(IDictionary<string, StringValues> parameters)
        {
            Parameters = parameters ?? throw new ArgumentNullException(nameof(parameters));
        }

        public DefaultRequestParameterAttribute(string query)
        {
            if (string.IsNullOrEmpty(query))
                throw new ArgumentException(nameof(query));

            Parameters = ClientUtilities.ParseNullableQuery(query);
        }

        #region Implementation of IRequestParameterProvider

        public IDictionary<string, StringValues> Parameters { get; }

        #endregion Implementation of IRequestParameterProvider
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    public class PathVariableAttribute : Attribute
    {
        public PathVariableAttribute(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public PathVariableAttribute()
        {
        }

        public string Name { get; }
    }

    [AttributeUsage(AttributeTargets.Parameter)]
    public class RequestParameterAttribute : Attribute
    {
        public RequestParameterAttribute(string name)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public RequestParameterAttribute()
        {
        }

        public string Name { get; }
    }

    /*    public interface IServiceNameProvider
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

        public interface IClientDefinitionProvider : IServiceNameProvider
        {
            string Host { get; }
            string Protocol { get; }
        }

        public interface IServiceIgnoreProvider { }

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
        public class NonServiceMethodAttribute : Attribute, IServiceIgnoreProvider { }*/
}