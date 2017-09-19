using Rabbit.Cloud.Facade.Abstractions;
using System;
using System.Collections.Generic;

namespace Rabbit.Cloud.Facade.Internal
{
    public class ServiceDescriptorCollection
    {
        public ServiceDescriptorCollection(IReadOnlyList<ServiceDescriptor> items, int version)
        {
            Items = items ?? throw new ArgumentNullException(nameof(items));
            Version = version;
        }

        public IReadOnlyList<ServiceDescriptor> Items { get; }

        public int Version { get; }
    }
}