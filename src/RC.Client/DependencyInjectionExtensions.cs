using Microsoft.Extensions.DependencyInjection;
using Rabbit.Cloud.Abstractions;
using Rabbit.Cloud.Client.Abstractions;
using Rabbit.Cloud.Client.Internal;
using System;

namespace Rabbit.Cloud.Client
{
    public static class DependencyInjectionExtensions
    {
        public static IRabbitCloudClient BuildRabbitCloudClient(this IServiceCollection services, Func<IServiceCollection, IServiceProvider, IServiceProvider> configureServices, Action<IRabbitApplicationBuilder> configure)
        {
            return new RabbitCloudClient(services, services.BuildServiceProvider(), configureServices, configure);
        }
    }
}