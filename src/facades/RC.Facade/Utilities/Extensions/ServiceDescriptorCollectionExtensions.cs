using Rabbit.Cloud.Facade.Abstractions;
using Rabbit.Cloud.Facade.Internal;
using System;
using System.Globalization;
using System.Linq;

namespace Rabbit.Cloud.Facade.Utilities.Extensions
{
    public static class ServiceDescriptorCollectionExtensions
    {
        public static ServiceDescriptor GetServiceDescriptor(this ServiceDescriptorCollection collection,
            IConvertible id)
        {
            var idString = id.ToString(CultureInfo.InvariantCulture);
            return collection.Items.SingleOrDefault(i => i.Id == idString);
        }
    }
}