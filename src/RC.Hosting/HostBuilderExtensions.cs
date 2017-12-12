using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rabbit.Cloud.Application;
using Rabbit.Cloud.Application.Abstractions;
using Rabbit.Extensions.Configuration;
using System;

namespace Rabbit.Cloud.Hosting
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder UseRabbitApplicationConfigure(this IHostBuilder hostBuilder)
        {
            return hostBuilder
                .ConfigureContainer<IServiceCollection>((ctx, services) =>
                {
                    var rabbitApplicationBuilder = new RabbitApplicationBuilder(services.BuildServiceProvider());
                    ctx.Properties["RabbitApplicationBuilder"] = rabbitApplicationBuilder;
                });
        }

        public static IHostBuilder ConfigureRabbitApplication(this IHostBuilder hostBuilder, Action<IRabbitApplicationBuilder> configure)
        {
            return hostBuilder.ConfigureRabbitApplication((ctx, app) =>
            {
                configure(app);
            });
        }

        public static IHostBuilder ConfigureRabbitApplication(this IHostBuilder hostBuilder, Action<HostBuilderContext, IRabbitApplicationBuilder> configure)
        {
            return hostBuilder
                .ConfigureContainer<IServiceCollection>((ctx, services) =>
                {
                    var app = (IRabbitApplicationBuilder)ctx.Properties["RabbitApplicationBuilder"];
                    configure(ctx, app);
                });
        }

        public static IHostBuilder BuidRabbitApp(this IHostBuilder hostBuilder)
        {
            return hostBuilder
                .ConfigureAppConfiguration((ctx, builder) =>
                {
                    ctx.Configuration.EnableTemplateSupport();
                })
                .ConfigureContainer<IServiceCollection>((ctx, services) =>
                {
                    var app = (IRabbitApplicationBuilder)ctx.Properties["RabbitApplicationBuilder"];
                    services.AddSingleton(app.Build());
                });
        }

        public static IHost BuidRabbitHost(this IHostBuilder hostBuilder)
        {
            return hostBuilder.BuidRabbitApp().Build();
        }
    }
}