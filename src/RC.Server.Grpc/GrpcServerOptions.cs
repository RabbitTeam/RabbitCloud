using Rabbit.Cloud.Application.Abstractions;

namespace Rabbit.Cloud.Server.Grpc
{
    public class GrpcServerOptions
    {
        public RabbitRequestDelegate Invoker { get; set; }
    }
}