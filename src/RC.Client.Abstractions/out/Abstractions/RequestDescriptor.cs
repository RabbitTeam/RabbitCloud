using System.Collections.Generic;

namespace Rabbit.Cloud.Client.Abstractions
{
    public class RequestDescriptor
    {
        public string Id { get; set; }
        public IList<RequestParameterDescriptor> Parameters { get; set; }
        public string DisplayName { get; set; }
        public string Url { get; set; }
        public IDictionary<object, object> Properties { get; set; } = new Dictionary<object, object>();
    }
}