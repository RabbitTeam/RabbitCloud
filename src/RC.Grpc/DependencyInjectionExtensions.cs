using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using Rabbit.Cloud.Abstractions.Utilities;
using Rabbit.Cloud.ApplicationModels;
using Rabbit.Cloud.Grpc.Abstractions;
using Rabbit.Cloud.Grpc.Abstractions.Client;
using Rabbit.Cloud.Grpc.Abstractions.Server;
using Rabbit.Cloud.Grpc.ApplicationModels;
using Rabbit.Cloud.Grpc.ApplicationModels.Internal;
using Rabbit.Cloud.Grpc.Client;
using Rabbit.Cloud.Grpc.Client.Internal;
using Rabbit.Cloud.Grpc.Internal;
using Rabbit.Cloud.Grpc.Server.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DefaultApplicationModelProvider = Rabbit.Cloud.Grpc.ApplicationModels.Internal.DefaultApplicationModelProvider;

namespace Rabbit.Cloud.Grpc
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddGrpcClient(this IServiceCollection services, Action<RabbitCloudOptions> configure = null)
        {
            return services
                .AddGrpcCore()
                .AddSingleton<ChannelPool>()
                .AddSingleton<ICallInvokerFactory, CallInvokerFactory>()
                .Configure<RabbitCloudOptions>(options =>
                {
                    foreach (var typeInfo in GetTypes(typePredicate: t => t.GetTypeAttribute<IClientDefinitionProvider>() != null))
                    {
                        options.ScanTypes.Add(typeInfo);
                    }
                    configure?.Invoke(options);
                });
        }

        public static IServiceCollection AddGrpcServer(this IServiceCollection services, Action<RabbitCloudOptions> configure = null)
        {
            return services
                .AddGrpcCore()
                .AddSingleton<IServerServiceDefinitionTableProvider, ApplicationModelServerServiceDefinitionTableProvider>()
                .Configure<RabbitCloudOptions>(options =>
                {
                    foreach (var typeInfo in GetTypes(typePredicate: t => t.GetTypeAttribute<IServiceDefinitionProvider>() != null))
                    {
                        options.ScanTypes.Add(typeInfo);
                    }
                    configure?.Invoke(options);
                });
        }

        private static IServiceCollection AddGrpcCore(this IServiceCollection services)
        {
            return services
                .AddSingleton<IMethodTableProvider, DefaultMethodTableProvider>()
                .AddSingleton<IApplicationModelProvider, DefaultApplicationModelProvider>()
                .AddSingleton<ApplicationModelHolder, ApplicationModelHolder>()
                .AddSingleton<IMethodProvider, MethodProvider>()
                .AddSingleton<IServerServiceDefinitionProvider, ServerServiceDefinitionProvider>()
                .AddSingleton<IServerMethodInvokerFactory, DefaultServerMethodInvokerFactory>()
                .AddSingleton<SerializerCacheTable, SerializerCacheTable>();
        }

        private static IEnumerable<TypeInfo> GetTypes(Func<AssemblyName, bool> assemblyPredicate = null, Func<TypeInfo, bool> typePredicate = null)
        {
            var assemblyNames = DependencyContext.Default.RuntimeLibraries.SelectMany(i => i.GetDefaultAssemblyNames(DependencyContext.Default));
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