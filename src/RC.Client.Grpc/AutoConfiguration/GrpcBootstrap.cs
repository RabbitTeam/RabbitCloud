using Microsoft.Extensions.Hosting;
using Rabbit.Cloud.Grpc;

namespace Rabbit.Cloud.Client.Grpc.AutoConfiguration
{
    public class Bootstrap
    {
        public static int Priority => 10;

        public static void Start(IHostBuilder hostBuilder)
        {
            hostBuilder
                .ConfigureServices(services =>
                {
                    services.AddGrpcClient();
                });
        }
    }
}