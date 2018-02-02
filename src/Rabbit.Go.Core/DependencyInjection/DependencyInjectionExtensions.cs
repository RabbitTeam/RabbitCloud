using Microsoft.Extensions.DependencyModel;
using Rabbit.Go;
using Rabbit.Go.Core;
using Rabbit.Go.Core.GoModels;
using Rabbit.Go.Core.Internal;
using Rabbit.Go.Formatters;
using Rabbit.Go.Internal;
using Rabbit.Go.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddGoClient(this IServiceCollection services)
        {
            return services
                .AddGoClient(options => { });
        }

        public static IServiceCollection AddGoClient(this IServiceCollection services, Action<GoOptions> configureOptions)
        {
            var types = GetTypes(typePredicate: t => t.IsInterface && t.GetTypeAttribute<GoAttribute>() != null).ToList();

            return services
                .AddGoClient(types, configureOptions);
        }

        public static IServiceCollection AddGoClient(this IServiceCollection services, IEnumerable<Type> goClienTypes, Action<GoOptions> configureOptions)
        {
            services
                .AddGo(options =>
                {
                    foreach (var type in goClienTypes)
                        options.Types.Add(type);

                    configureOptions?.Invoke(options);
                });

            foreach (var type in goClienTypes)
            {
                services.AddSingleton(type, s =>
                {
                    var goFactory = s.GetRequiredService<IGoFactory>();
                    return goFactory.CreateInstance(type);
                });
            }

            return services;
        }

        public static IServiceCollection AddGo(this IServiceCollection serviceCollection, Action<GoOptions> configureOptions)
        {
            serviceCollection
                .Configure(configureOptions)
                .AddSingleton<IKeyValueFormatterFactory, KeyValueFormatterFactory>()
                .AddSingleton<IMethodDescriptorCollectionProvider, MethodDescriptorCollectionProvider>()
                .AddSingleton<IMethodDescriptorProvider, GoModelMethodDescriptorProvider>()
                .AddSingleton<ITemplateParser, TemplateParser>()
                .AddSingleton<IMethodInvokerFactory, DefaultMethodInvokerFactory>()
                .AddSingleton<MethodInvokerCache>()
                .AddSingleton<IGoModelProvider, DefaultGoModelProvider>()
                .AddSingleton<IGoFactory, DefaultGoGoFactory>();

            return serviceCollection;
        }

        private static IEnumerable<Type> GetTypes(Func<AssemblyName, bool> assemblyPredicate = null, Func<Type, bool> typePredicate = null)
        {
            var assemblyNames = DependencyContext.Default.RuntimeLibraries
                .SelectMany(i => i.GetDefaultAssemblyNames(DependencyContext.Default));
            if (assemblyPredicate != null)
                assemblyNames = assemblyNames.Where(assemblyPredicate).ToArray();
            var assemblies = assemblyNames.Select(i => Assembly.Load(new AssemblyName(i.Name))).ToArray();

            var types = assemblies.SelectMany(i => i.GetExportedTypes());
            if (typePredicate != null)
                types = types.Where(typePredicate);

            return types.ToArray();
        }
    }
}