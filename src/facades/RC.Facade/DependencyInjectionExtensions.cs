using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Rabbit.Cloud.Facade.Abstractions;

namespace Rabbit.Cloud.Facade
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddFacade(this IServiceCollection services)
        {
            services
                .AddTransient<IConfigureOptions<FacadeOptions>, FacadeOptionsSetup>()
                .AddTransient<ProxyFactory, ProxyFactory>();
            return services;
        }
    }
}