using Rabbit.Cloud.Application;
using Rabbit.Cloud.Application.Abstractions;
using System;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
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
            var invoker = app.Build();
            return services
                .AddSingleton(invoker);
            //                .AddSingleton<IRabbitClient>(new RabbitClient(invoker, app.ApplicationServices));
        }
    }
}