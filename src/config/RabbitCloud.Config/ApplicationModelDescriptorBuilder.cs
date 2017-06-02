using Microsoft.Extensions.Configuration;
using RabbitCloud.Config.Abstractions;
using RabbitCloud.Config.Abstractions.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RabbitCloud.Config
{
    public class ApplicationModelDescriptorBuilder
    {
        private readonly ApplicationModelDescriptor _applicationModelDescriptor = new ApplicationModelDescriptor
        {
            Protocols = new ProtocolConfig[0],
            Registrys = new RegistryConfig[0],
            Referers = new RefererConfig[0],
            Services = new ServiceConfig[0]
        };

        private readonly IList<IConfiguration> _configurations = new List<IConfiguration>();

        public ApplicationModelDescriptorBuilder AddConfiguration(IConfiguration configuration)
        {
            _configurations.Add(configuration);

            return this;
        }

        public ApplicationModelDescriptorBuilder AddReferer(Type serviceType)
        {
            var refererAttribute = serviceType.GetTypeInfo().GetCustomAttribute<RefererAttribute>();
            var config = new RefererConfig
            {
                Cluster = refererAttribute.Cluster,
                Group = refererAttribute.Group,
                HaStrategy = refererAttribute.HaStrategy,
                Interface = serviceType.AssemblyQualifiedName,
                LoadBalance = refererAttribute.LoadBalance,
                Protocol = refererAttribute.Protocol,
                Registry = refererAttribute.Registry
            };

            _applicationModelDescriptor.Referers = _applicationModelDescriptor.Referers.Concat(new[] { config })
                .ToArray();

            return this;
        }

        public ApplicationModelDescriptor Build()
        {
            foreach (var configuration in _configurations)
            {
                var descriptor = new ApplicationModelDescriptor();
                configuration.Bind(descriptor);

                if (descriptor.Protocols != null)
                    _applicationModelDescriptor.Protocols = descriptor.Protocols.Concat(_applicationModelDescriptor.Protocols)
                        .ToArray();
                if (descriptor.Registrys != null)
                    _applicationModelDescriptor.Registrys = descriptor.Registrys.Concat(_applicationModelDescriptor.Registrys)
                        .ToArray();
                if (descriptor.Referers != null)
                    _applicationModelDescriptor.Referers = descriptor.Referers.Concat(_applicationModelDescriptor.Referers)
                        .ToArray();
                if (descriptor.Services != null)
                    _applicationModelDescriptor.Services = descriptor.Services.Concat(_applicationModelDescriptor.Services)
                        .ToArray();
            }

            return _applicationModelDescriptor;
        }
    }
}