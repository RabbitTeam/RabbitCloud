using Microsoft.Extensions.Options;
using Rabbit.Cloud.Grpc.Abstractions.Method;
using System;
using System.Linq;
using System.Reflection;

namespace Rabbit.Cloud.Grpc.Abstractions.Internal
{
    public class MethodProviderOptions
    {
        public MethodInfo[] MethodInfos { get; set; }
        public Func<Type, object> MarshallerFactory { get; set; }
    }

    public class DefaultMethodProvider : IMethodProvider
    {
        private readonly MethodProviderOptions _options;

        public DefaultMethodProvider(IOptions<MethodProviderOptions> options)
        {
            _options = options.Value;
        }

        #region Implementation of IMethodProvider

        public void Collect(IMethodCollection methods)
        {
            if (_options.MethodInfos == null || !_options.MethodInfos.Any())
                return;

            foreach (var methodInfo in _options.MethodInfos)
            {
                var descriptor = GrpcServiceDescriptor.Create(methodInfo);

                var item = methods.Get(descriptor.ServiceId);
                if (item != null)
                    continue;

                descriptor.RequestMarshaller = _options.MarshallerFactory(descriptor.RequesType);
                descriptor.ResponseMarshaller = _options.MarshallerFactory(descriptor.ResponseType);

                item = descriptor.CreateMethod();

                methods.Set(item);
            }
        }

        #endregion Implementation of IMethodProvider
    }
}