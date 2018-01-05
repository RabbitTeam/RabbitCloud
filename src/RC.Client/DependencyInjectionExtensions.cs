using Microsoft.Extensions.DependencyInjection;
using Rabbit.Cloud.Application;
using Rabbit.Cloud.Application.Abstractions;
using System;

namespace Rabbit.Cloud.Client
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddRabbitClient(this IServiceCollection services,
            IServiceProvider applicationServices, Action<IRabbitApplicationBuilder> configure)
        {
            var app = new RabbitApplicationBuilder(applicationServices);
            configure(app);
            return services
                .AddRabbitClient(app);
        }

        public static IServiceCollection AddRabbitClient(this IServiceCollection services, IRabbitApplicationBuilder app)
        {
            return services
                .AddSingleton<IRabbitClient>(new RabbitClient(app.Build(), app.ApplicationServices));
        }
    }
}