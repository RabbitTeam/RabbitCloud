using Microsoft.Extensions.DependencyModel;
using Rabbit.Go;
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
        public static IServiceCollection AddGo(this IServiceCollection serviceCollection)
        {
            return serviceCollection
                .AddGo(options =>
                {
                    foreach (var type in GetTypes(typePredicate: t =>
                        t.IsInterface && t.GetTypeAttribute<GoAttribute>() != null))
                    {
                        options.Types.Add(type);
                    }
                });
        }

        public static IServiceCollection AddGo(this IServiceCollection serviceCollection, IEnumerable<Type> types)
        {
            return serviceCollection
                .AddGo(options =>
                {
                    foreach (var type in types)
                    {
                        options.Types.Add(type);
                    }
                });
        }

        public static IServiceCollection AddGo(this IServiceCollection serviceCollection, Action<GoOptions> configureOptions)
        {
            return serviceCollection
                .Configure(configureOptions)
                .AddSingleton<IKeyValueFormatterFactory, KeyValueFormatterFactory>()
                .AddSingleton<IMethodDescriptorCollectionProvider, MethodDescriptorCollectionProvider>()
                .AddSingleton<IMethodDescriptorProvider, DefaultMethodDescriptorProvider>()
                .AddSingleton<IGoFactory, DefaultGoFactory>()
                .AddSingleton<IGoClient, HttpGoClient>()
                .AddSingleton<MethodInvokerProvider>()
                .AddSingleton<AsynchronousMethodInvokerCache>()
                .AddSingleton<ITemplateParser, TemplateParser>();
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