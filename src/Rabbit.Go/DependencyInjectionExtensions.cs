using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Options;
using Rabbit.Cloud.Application;
using Rabbit.Cloud.Application.Abstractions;
using Rabbit.Go;
using Rabbit.Go.Abstractions;
using Rabbit.Go.ApplicationModels;
using Rabbit.Go.ApplicationModels.Internal;
using Rabbit.Go.Internal;
using System;
using System.Linq;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddGoClient(this IServiceCollection serviceCollection,
            Action<IRabbitApplicationBuilder> appBuilder)
        {
            var appServices = serviceCollection
                .AddSingleton<IApplicationModelProvider, DefaultApplicationModelProvider>()
                .BuildServiceProvider();

            var types = GetTypes(null, type => type.GetCustomAttribute<GoClientAttribute>() != null);

            var providers = appServices.GetServices<IApplicationModelProvider>();
            var applicationModel = ApplicationModelUtilities.BuildModel(types, providers);

            var goOptions = appServices.GetRequiredService<IOptions<GoOptions>>().Value;
            var conventions = goOptions.Conventions;

            ApplicationModelUtilities.ApplyConventions(applicationModel, conventions);

            return serviceCollection
                .AddGoClient(applicationModel, appServices, appBuilder);
        }

        public static IServiceCollection AddGoClient(this IServiceCollection serviceCollection,
            ApplicationModel applicationModel, IServiceProvider services, Action<IRabbitApplicationBuilder> appBuilder)
        {
            var applicationBuilder = new RabbitApplicationBuilder(services);
            appBuilder(applicationBuilder);

            var app = applicationBuilder.Build();

            var proxyFactory = new ProxyFactory(new[] { new RabbitGoInterceptor(app, applicationModel) });

            serviceCollection
                .AddSingleton<IProxyFactory>(proxyFactory);

            foreach (var serviceModel in applicationModel.Services)
            {
                serviceCollection
                    .AddSingleton(serviceModel.Type, s => proxyFactory.CreateProxy(serviceModel.Type));
            }

            return serviceCollection;
        }

        private static TypeInfo[] GetTypes(Func<AssemblyName, bool> assemblyPredicate = null, Func<TypeInfo, bool> typePredicate = null)
        {
            var assemblyNames = DependencyContext.Default.RuntimeLibraries
                .SelectMany(i => i.GetDefaultAssemblyNames(DependencyContext.Default));
            if (assemblyPredicate != null)
                assemblyNames = assemblyNames.Where(assemblyPredicate).ToArray();
            var assemblies = assemblyNames.Select(i => Assembly.Load(new AssemblyName(i.Name))).ToArray();

            var types = assemblies.SelectMany(i => i.GetExportedTypes().Select(t => t.GetTypeInfo()));
            if (typePredicate != null)
                types = types.Where(typePredicate);

            return types.ToArray();
        }
    }
}