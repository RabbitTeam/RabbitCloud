using Microsoft.Extensions.Options;
using Rabbit.Cloud.Grpc.Abstractions.Method;
using System;
using System.Reflection;

namespace Rabbit.Cloud.Grpc.Abstractions.Internal
{
    public class DefaultMethodProviderOptions
    {
        public Type[] Types { get; set; }
        public Func<Type, object> MarshallerFactory { get; set; }
    }

    public class DefaultMethodProvider : IMethodProvider
    {
        private readonly DefaultMethodProviderOptions _options;

        public DefaultMethodProvider(IOptions<DefaultMethodProviderOptions> options)
        {
            _options = options.Value;
        }

        #region Implementation of IMethodProvider

        public void Collect(IMethodCollection methods)
        {
            foreach (var type in _options.Types)
            {
                var methodInfos = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                foreach (var methodInfo in methodInfos)
                {
                    var descriptor = GrpcServiceDescriptor.Create(methodInfo, _options.MarshallerFactory);
                    methods.Set(descriptor.CreateMethod());
                }
            }
        }

        #endregion Implementation of IMethodProvider
    }
}