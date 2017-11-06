using Microsoft.Extensions.Options;
using Rabbit.Cloud.Grpc.Abstractions.Method;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Rabbit.Cloud.Grpc.Abstractions.Internal
{
    public class MethodProviderOptions
    {
        public IDictionary<Type, MethodInfo[]> Entries { get; set; }
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
            if (_options.Entries == null || !_options.Entries.Any())
                return;

            foreach (var entry in _options.Entries)
            {
                var type = entry.Key;
                foreach (var methodInfo in entry.Value)
                {
                    var descriptor = GrpcServiceDescriptor.Create(type, methodInfo);

                    var item = methods.Get(descriptor.ServiceId);
                    if (item != null)
                        continue;

                    descriptor.RequestMarshaller = _options.MarshallerFactory(descriptor.RequesType);
                    descriptor.ResponseMarshaller = _options.MarshallerFactory(descriptor.ResponseType);

                    item = descriptor.CreateMethod();

                    methods.Set(item);
                }
            }
        }

        #endregion Implementation of IMethodProvider
    }
}