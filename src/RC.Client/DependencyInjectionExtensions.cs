using Microsoft.Extensions.DependencyInjection;
using Rabbit.Cloud.Abstractions;
using Rabbit.Cloud.Client.Abstractions;
using Rabbit.Cloud.Client.Internal;
using System;
using System.Net.Http;

namespace Rabbit.Cloud.Client
{
    public static class DependencyInjectionExtensions
    {
        public static IRabbitBuilder AddRabbitCloudClient(this IRabbitBuilder builder, Action<HttpClient> configure = null)
        {
            return builder.AddRabbitCloudClient(() =>
            {
                var client = new HttpClient();
                configure?.Invoke(client);
                return client;
            });
        }

        public static IRabbitBuilder AddRabbitCloudClient(this IRabbitBuilder builder, Func<HttpClient> clientFactory)
        {
            builder.Services.AddSingleton(clientFactory());
            return builder;
        }

        public static IRabbitCloudClient BuildRabbitCloudClient(this IServiceCollection services, Func<IServiceCollection, IServiceProvider, IServiceProvider> configureServices, Action<IRabbitApplicationBuilder> configure)
        {
            return new RabbitCloudClient(services, services.BuildServiceProvider(), configureServices, configure);
        }
    }
}