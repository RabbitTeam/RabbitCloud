using System.Collections.Generic;

namespace Rabbit.Cloud.Client.Abstractions
{
    public class RequestDescriptorProviderContext
    {
        public IList<RequestDescriptor> Results { get; } = new List<RequestDescriptor>();
    }
}