using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using Rabbit.Cloud.Abstractions.Utilities;
using Rabbit.Cloud.Grpc.Abstractions.Adapter;
using Rabbit.Cloud.Grpc.Abstractions.ApplicationModels;
using Rabbit.Cloud.Grpc.Abstractions.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Rabbit.Cloud.Grpc.Abstractions
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddGrpcCore(this IServiceCollection services, Func<AssemblyName, bool> assemblyPredicate = null, Func<MethodInfo, bool> methodPredicate = null, Func<Type, bool> typePredicate = null)
        {
            var assemblyNames = DependencyContext.Default.RuntimeLibraries.SelectMany(i => i.GetDefaultAssemblyNames(DependencyContext.Default));
            if (assemblyPredicate != null)
                assemblyNames = assemblyNames.Where(assemblyPredicate).ToArray();
            var assemblies = assemblyNames.Select(i => Assembly.Load(new AssemblyName(i.Name))).ToArray();

            var types = assemblies.SelectMany(i => i.GetExportedTypes());
            if (typePredicate != null)
                types = types.Where(typePredicate);

            types = types.Where(t => t.GetTypeAttribute<IGrpcDefinitionProvider>() != null);

            var entries = types.ToDictionary(t => t, t => t.GetMethods().Where(m => m.DeclaringType != typeof(object) && (methodPredicate == null || methodPredicate(m))).ToArray());

            return services
                .AddGrpcCore(options =>
                {
                    options.Entries = entries;
                });
        }

        public static IServiceCollection AddGrpcCore(this IServiceCollection services, Action<MethodProviderOptions> configure)
        {
            return services
                .Configure<MethodProviderOptions>(options =>
                {
                    options.MarshallerFactory = type =>
                    {
                        var jsonCodec = new JsonCodec();
                        return new Marshaller(type, jsonCodec.Encode, jsonCodec.Decode);
                    };
                    configure?.Invoke(options);
                })
                .AddSingleton<IGrpcServiceDescriptorProvider, DefaultGrpcServiceDescriptorProvider>()
                .AddSingleton<IGrpcServiceDescriptorCollection>(s =>
                {
                    var providers = s.GetRequiredService<IEnumerable<IGrpcServiceDescriptorProvider>>();
                    var descriptors = new GrpcServiceDescriptorCollection();

                    foreach (var provider in providers)
                    {
                        provider.Collect(descriptors);
                    }
                    return descriptors;
                });
        }
    }
}