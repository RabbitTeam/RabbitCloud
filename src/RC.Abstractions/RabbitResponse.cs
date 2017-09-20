using System.Net.Http;

namespace Rabbit.Cloud.Abstractions
{
    public abstract class RabbitResponse
    {
        public abstract RabbitContext RabbitContext { get; }
        public abstract HttpResponseMessage ResponseMessage { get; set; }
    }
}