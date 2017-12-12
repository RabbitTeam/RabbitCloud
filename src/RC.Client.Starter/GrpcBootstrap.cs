using Microsoft.Extensions.Hosting;
using Rabbit.Cloud.Application;
using Rabbit.Cloud.Client.Grpc.Builder;
using Rabbit.Cloud.Grpc;

namespace Rabbit.Cloud.Client.Starter
{
    public class GrpcBootstrap
    {
        public static int Priority => 10;

        public static void Start(IHostBuilder hostBuilder)
        {
            hostBuilder
                .ConfigureServices(services =>
                {
                    services.AddGrpcClient();
                })
                .ConfigureRabbitApplication(app =>
                {
                    app.UseGrpc();
                });
        }
    }
}