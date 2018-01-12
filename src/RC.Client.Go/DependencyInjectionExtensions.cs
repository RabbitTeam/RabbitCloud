using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using Microsoft.Extensions.Options;
using Rabbit.Cloud.Client.Go.Abstractions;
using Rabbit.Cloud.Client.Go.ApplicationModels;
using Rabbit.Cloud.Client.Go.Internal;
using System;
using System.Linq;
using System.Reflection;

namespace Rabbit.Cloud.Client.Go
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddGoClientProxy(this IServiceCollection services, params string[] assemblyPrefixs)
        {
            return services
                    .AddApplicationModel(assemblyPrefixs)
                    .AddSingleton<IInterceptor, DefaultGoInterceptor>()
                    .AddGoClient();
        }

        private static IServiceCollection AddApplicationModel(this IServiceCollection services, params string[] assemblyPrefixs)
        {
            return services
                .AddSingleton(s =>
                {
                    Func<AssemblyName, bool> assemblyPredicate = null;
                    if (assemblyPrefixs != null && assemblyPrefixs.Any())
                        assemblyPredicate = i => assemblyPrefixs.Any(prefix => i.Name.StartsWith(prefix));

                    var types = GetTypes(assemblyPredicate, type => type.GetCustomAttribute<GoClientAttribute>() != null);

                    var providers = s.GetServices<IApplicationModelProvider>();

                    var applicationModel = ApplicationModelUtilities.BuildModel(types, providers);

                    var goOptions = s.GetRequiredService<IOptions<GoOptions>>().Value;
                    var conventions = goOptions.Conventions;

                    ApplicationModelUtilities.ApplyConventions(applicationModel, conventions);

                    return applicationModel;
                });
        }

        public static IServiceCollection InjectionServiceProxy(this IServiceCollection services, ApplicationModel applicationModel)
        {
            foreach (var service in applicationModel.Services)
            {
                var type = service.Type;
                services.AddSingleton(type, p => p.GetRequiredService<IProxyFactory>().CreateProxy(type));
            }

            return services;
        }

        public static IServiceCollection AddGoClient(this IServiceCollection services)
        {
            return services
                .AddSingleton<IApplicationModelProvider, DefaultApplicationModelProvider>()
                .AddSingleton<IProxyFactory, ProxyFactory>()
                .AddSingleton<ITemplateEngine, TemplateEngine>();
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