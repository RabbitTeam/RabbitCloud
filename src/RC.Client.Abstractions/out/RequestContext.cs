using Rabbit.Cloud.Application.Abstractions;

namespace Rabbit.Cloud.Client.Abstractions
{
    public class RequestContext
    {
        public RequestDescriptor RequestDescriptor { get; set; }
        public IRabbitContext RabbitContext { get; set; }
    }
}