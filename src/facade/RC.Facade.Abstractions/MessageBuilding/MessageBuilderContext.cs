using System.Collections.Generic;

namespace Rabbit.Cloud.Facade.Abstractions.MessageBuilding
{
    public class MessageBuilderContext
    {
        public MessageBuilderContext(ServiceRequestContext serviceRequestContext)
        {
            ServiceRequestContext = serviceRequestContext;
            Querys = new List<KeyValuePair<string, string>>();
            Headers = new List<KeyValuePair<string, string>>();
            Forms = new List<KeyValuePair<string, string>>();
        }

        public ServiceRequestContext ServiceRequestContext { get; }

        public ParameterDescriptor ParameterDescriptor { get; set; }

        public ICollection<KeyValuePair<string, string>> Querys { get; }
        public ICollection<KeyValuePair<string, string>> Headers { get; }
        public ICollection<KeyValuePair<string, string>> Forms { get; }
    }
}