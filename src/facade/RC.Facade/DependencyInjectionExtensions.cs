using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;
using Rabbit.Cloud.Client.Abstractions;
using Rabbit.Cloud.Facade.Abstractions;
using Rabbit.Cloud.Facade.Abstractions.Abstractions;
using Rabbit.Cloud.Facade.Abstractions.MessageBuilding;
using Rabbit.Cloud.Facade.Internal;
using Rabbit.Cloud.Facade.MessageBuilding.Builders;
using Rabbit.Cloud.Facade.Models;
using Rabbit.Cloud.Facade.Models.Internal;
using System.Buffers;

namespace Rabbit.Cloud.Facade
{
    public static class DependencyInjectionExtensions
    {
        public static IFacadeBuilder AddFacadeCore(this IRabbitBuilder builder)
        {
            var services = builder.Services;
            services
                .AddSingleton(ArrayPool<char>.Shared)
                .AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>()
                .AddSingleton<IServiceDescriptorCollectionProvider, ServiceDescriptorCollectionProvider>()
                .AddSingleton<IServiceDescriptorProvider, ApplicationServiceDescriptorProvider>()
                .AddSingleton<IApplicationModelProvider, DefaultApplicationModelProvider>()
                .AddSingleton<IMessageBuilder, BasicMessageBuilder>()
                .AddSingleton<IMessageBuilder, ToBodyMessageBuilder>()
                .AddSingleton<IRequestMessageBuilder, RequestMessageBuilder>()
                .AddSingleton<IProxyFactory, ProxyFactory>();

            var facadeBuilder = new FacadeBuilder(builder);
            return facadeBuilder;
        }

/*        public static IServiceCollection InjectionFacadeClient(this IServiceCollection services, IRabbitCloudClient rabbitCloudClient)
        {
            var rabbitServices = rabbitCloudClient.Services;

            var proxyFactory = rabbitServices.GetRequiredService<IProxyFactory>();
            foreach (var facadeType in ApplicationServiceDescriptorProvider.GetFacadeTypes())
            {
                services.AddSingleton(facadeType, proxyFactory.GetProxy(facadeType, rabbitCloudClient.RequestAsync));
            }

            return services;
        }*/
    }
}