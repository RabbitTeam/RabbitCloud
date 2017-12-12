using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rabbit.Cloud.Abstractions;

namespace RC.Starter
{
    public static class RabbitBootstrap
    {
        public static void Start(IHostBuilder hostBuilder)
        {
            hostBuilder
                .ConfigureServices((ctx, services) =>
                {
                    var section = ctx.Configuration.GetSection("RabbitCloud");
                    services.Configure<RabbitApplicationOptions>(section);
                });
        }
    }
}