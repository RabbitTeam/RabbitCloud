using System.IO;

namespace Rabbit.Cloud.Client.Abstractions
{
    public abstract class RabbitResponse<TContext>
    {
        public abstract TContext RabbitContext { get; }
        public abstract int StatusCode { get; set; }
        public abstract Stream Body { get; set; }
    }
}