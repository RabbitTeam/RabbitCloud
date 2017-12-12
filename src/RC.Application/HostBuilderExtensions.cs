using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rabbit.Cloud.Application.Abstractions;
using System;

namespace Rabbit.Cloud.Application
{
    public static class HostBuilderExtensions
    {
        public static IHostBuilder UseRabbitApplicationConfigure(this IHostBuilder hostBuilder)
        {
            return hostBuilder.ConfigureContainer<IServiceCollection>((ctx, services) =>
            {
                var applicationServices = services.BuildServiceProvider();
                ctx.Properties["ApplicationServices"] = applicationServices;
                ctx.Properties["RabbitApplicationBuilder"] = new RabbitApplicationBuilder(applicationServices);
            });
        }

        public static IHostBuilder ConfigureRabbitApplication(this IHostBuilder hostBuilder, Action<IRabbitApplicationBuilder> configure)
        {
            return hostBuilder.ConfigureRabbitApplication((ctx, services, app) =>
            {
                configure(app);
            });
        }

        public static IHostBuilder ConfigureRabbitApplication(this IHostBuilder hostBuilder, Action<HostBuilderContext, IServiceCollection, IRabbitApplicationBuilder> configure)
        {
            return hostBuilder.ConfigureContainer<IServiceCollection>((ctx, services) =>
            {
                var app = (IRabbitApplicationBuilder)ctx.Properties["RabbitApplicationBuilder"];
                configure(ctx, services, app);
            });
        }
    }
}