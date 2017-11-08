using Microsoft.Extensions.Options;
using Rabbit.Cloud.Grpc.Abstractions.Adapter;
using Rabbit.Cloud.Grpc.Abstractions.ApplicationModels;
using Rabbit.Cloud.Grpc.Abstractions.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Rabbit.Cloud.Grpc.Abstractions.Internal
{
    public class MethodProviderOptions
    {
        public IDictionary<Type, MethodInfo[]> Entries { get; set; }
        public Func<Type, Marshaller> MarshallerFactory { get; set; }
    }

    public class DefaultGrpcServiceDescriptorProvider : IGrpcServiceDescriptorProvider
    {
        private readonly MethodProviderOptions _options;

        public DefaultGrpcServiceDescriptorProvider(IOptions<MethodProviderOptions> options)
        {
            _options = options.Value;
        }

        #region Implementation of IMethodProvider

        public void Collect(IGrpcServiceDescriptorCollection serviceDescriptors)
        {
            if (_options.Entries == null || !_options.Entries.Any())
                return;

            foreach (var entry in _options.Entries)
            {
                var type = entry.Key;
                foreach (var methodInfo in entry.Value)
                {
                    var descriptor = GrpcServiceDescriptor.Create(type, methodInfo);

                    var item = serviceDescriptors.Get(descriptor.ServiceId);
                    if (item != null)
                        continue;

                    descriptor.RequestMarshaller = _options.MarshallerFactory(methodInfo.GetRequestType());
                    descriptor.ResponseMarshaller = _options.MarshallerFactory(methodInfo.GetResponseType());

                    item = descriptor;

                    serviceDescriptors.Set(item.ServiceId, item);
                }
            }
        }

        #endregion Implementation of IMethodProvider
    }
}