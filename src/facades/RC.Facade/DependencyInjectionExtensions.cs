using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;
using Rabbit.Cloud.Facade.Abstractions;
using Rabbit.Cloud.Facade.Internal;
using System.Buffers;

namespace Rabbit.Cloud.Facade
{
    public static class DependencyInjectionExtensions
    {
        public static IFacadeBuilder AddFacadeCore(this IServiceCollection services)
        {
            services
                .AddSingleton(ArrayPool<char>.Shared)
                .AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>()
                .AddOptions()
                .AddLogging();

            var builder = new FacadeBuilder(services);
            return builder;
        }
    }
}