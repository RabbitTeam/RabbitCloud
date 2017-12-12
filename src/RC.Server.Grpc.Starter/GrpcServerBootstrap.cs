using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rabbit.Cloud.Application;
using Rabbit.Cloud.Grpc;
using Rabbit.Cloud.Server.Grpc.Builder;

namespace Rabbit.Cloud.Server.Grpc.Starter
{
    public class GrpcServerOptions
    {
        public string Host { get; set; }
        public int Port { get; set; }
    }

    public class GrpcServerBootstrap
    {
        public static int Priority => 10;

        public static void Start(IHostBuilder hostBuilder)
        {
            hostBuilder
                .ConfigureServices((ctx, services) =>
                {
                    var grpConfiguration = ctx.Configuration.GetSection("RabbitCloud:Server:Grpc");
                    if (!grpConfiguration.Exists())
                        return;
                    services
                        .Configure<GrpcServerOptions>(grpConfiguration)
                        .AddGrpcServer()
                        .AddServerGrpc()
                        .AddSingleton<IHostedService, GrpcServerHostedService>();
                })
                .ConfigureRabbitApplication(app =>
                {
                    app.UseServerGrpc();
                });
        }
    }
}