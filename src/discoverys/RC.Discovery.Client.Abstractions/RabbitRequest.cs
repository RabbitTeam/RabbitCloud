using System.Net.Http;

namespace RC.Discovery.Client.Abstractions
{
    public abstract class RabbitRequest
    {
        public abstract RabbitContext RabbitContext { get; }
        public abstract HttpRequestMessage RequestMessage { get; set; }
    }
}