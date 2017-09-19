using System.Net.Http;

namespace RC.Abstractions
{
    public abstract class RabbitRequest
    {
        public abstract RabbitContext RabbitContext { get; }
        public abstract HttpRequestMessage RequestMessage { get; set; }
    }
}