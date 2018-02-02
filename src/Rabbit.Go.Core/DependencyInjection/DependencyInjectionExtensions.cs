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
using System.Net.Http;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class DependencyInjectionExtensions
    {
        /*
                public static IServiceCollection AddGo(this IServiceCollection services, GoOptions options)
                {
                    return services
                        .AddSingleton(Options.Options.Create(options))
                        .InjectionGoClient(options.Types);
                }
        */

        public static IServiceCollection InjectionGoClient(this IServiceCollection services)
        {
            var types = GetTypes(typePredicate: t => t.IsInterface && t.GetTypeAttribute<GoAttribute>() != null).ToList();

            return services.InjectionGoClient(types);
        }

        public static IServiceCollection InjectionGoClient(this IServiceCollection services, IEnumerable<Type> types)
        {
            services
                .AddGo(options =>
                {
                    foreach (var type in types)
                        options.Types.Add(type);
                });

            foreach (var type in types)
            {
                services.AddSingleton(type, s =>
                {
                    var go = s.GetRequiredService<Go>();
                    return go.CreateInstance(type);
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
                .AddSingleton<HttpClient>()
                .AddSingleton<IGoModelProvider, DefaultGoModelProvider>()
                .AddSingleton<Go, DefaultGo>();

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