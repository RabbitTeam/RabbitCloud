using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using Rabbit.Cloud.Client.Go.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Rabbit.Cloud.Client.Go
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddGoClientProxy(this IServiceCollection services, params string[] assemblyPrefixs)
        {
            services
                .AddGoClient();

            Func<AssemblyName, bool> assemblyPredicate = null;
            if (assemblyPrefixs != null && assemblyPrefixs.Any())
                assemblyPredicate = i => assemblyPrefixs.Any(prefix => i.Name.StartsWith(prefix));

            var types = GetTypes(assemblyPredicate, type => type.GetCustomAttribute<GoClientAttribute>() != null);

            return services.InjectionServiceProxy(types);
        }

        public static IServiceCollection InjectionServiceProxy(this IServiceCollection services, IEnumerable<Type> types)
        {
            foreach (var type in types)
                services.AddSingleton(type, p => p.GetRequiredService<IProxyFactory>().CreateProxy(type));

            return services;
        }

        public static IServiceCollection AddGoClient(this IServiceCollection services)
        {
            return services
                .AddSingleton<IProxyFactory, ProxyFactory>()
                .AddSingleton<ITemplateEngine, TemplateEngine>();
        }

        private static IEnumerable<TypeInfo> GetTypes(Func<AssemblyName, bool> assemblyPredicate = null, Func<TypeInfo, bool> typePredicate = null)
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