using System.Net.Http;

namespace RC.Abstractions
{
    public abstract class RabbitResponse
    {
        public abstract RabbitContext RabbitContext { get; }
        public abstract HttpResponseMessage ResponseMessage { get; set; }
    }
}