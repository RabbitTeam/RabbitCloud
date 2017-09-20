using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;
using Rabbit.Cloud.Abstractions;
using Rabbit.Cloud.Facade.Abstractions;
using Rabbit.Cloud.Facade.Abstractions.Abstractions;
using Rabbit.Cloud.Facade.Internal;
using Rabbit.Cloud.Facade.Models;
using Rabbit.Cloud.Facade.Models.Internal;
using System.Buffers;
using System.Net.Http;

namespace Rabbit.Cloud.Facade
{
    public static class DependencyInjectionExtensions
    {
        public static IFacadeBuilder AddFacadeCore(this IRabbitBuilder builder)
        {
            var services = builder.Services;
            services
                .AddSingleton(new HttpClient())
                .AddSingleton(ArrayPool<char>.Shared)
                .AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>()
                .AddSingleton<IServiceDescriptorCollectionProvider, ServiceDescriptorCollectionProvider>()
                .AddSingleton<IServiceDescriptorProvider, ApplicationServiceDescriptorProvider>()
                .AddSingleton<IApplicationModelProvider, DefaultApplicationModelProvider>()
                .AddSingleton<IRequestMessageBuilder, RequestMessageBuilder>()
                .AddSingleton<IProxyFactory, ProxyFactory>()
                .AddOptions()
                .AddLogging();

            var facadeBuilder = new FacadeBuilder(services);
            return facadeBuilder;
        }
    }
}