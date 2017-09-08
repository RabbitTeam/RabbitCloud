/*using Microsoft.Extensions.DependencyInjection;
using Rabbit.Cloud.Facade.Abstractions;
using RC.Discovery.Client.Abstractions;

namespace Rabbit.Cloud.Facade
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddFacade(this IServiceCollection services, RabbitRequestDelegate rabbitRequestDelegate)
        {
            services
                .AddSingleton<IProxyFactory>(new ProxyFactory(rabbitRequestDelegate));
            return services;
        }
    }
}*/