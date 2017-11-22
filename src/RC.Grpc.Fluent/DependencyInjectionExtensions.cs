using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using Rabbit.Cloud.Abstractions.Utilities;
using Rabbit.Cloud.Grpc.Abstractions;
using Rabbit.Cloud.Grpc.Fluent.ApplicationModels;
using Rabbit.Cloud.Grpc.Fluent.ApplicationModels.Internal;
using Rabbit.Cloud.Grpc.Fluent.Internal;
using Rabbit.Cloud.Grpc.Server;
using System;
using System.Linq;
using System.Reflection;

namespace Rabbit.Cloud.Grpc.Fluent
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddGrpcFluent(this IServiceCollection services,
            Action<ApplicationModelOptions> configure)
        {
            return services
                .Configure(configure)
                .InternalAddGrpcFluent();
        }

        public static IServiceCollection AddGrpcFluent(this IServiceCollection services, Func<AssemblyName, bool> assemblyPredicate = null, Func<TypeInfo, bool> typePredicate = null)
        {
            var assemblyNames = DependencyContext.Default.RuntimeLibraries.SelectMany(i => i.GetDefaultAssemblyNames(DependencyContext.Default));
            if (assemblyPredicate != null)
                assemblyNames = assemblyNames.Where(assemblyPredicate).ToArray();
            var assemblies = assemblyNames.Select(i => Assembly.Load(new AssemblyName(i.Name))).ToArray();

            var types = assemblies.SelectMany(i => i.GetExportedTypes().Select(t => t.GetTypeInfo()));
            if (typePredicate != null)
                types = types.Where(typePredicate);

            types = types.Where(t => t.GetTypeAttribute<IGrpcDefinitionProvider>() != null).ToArray();

            return services.AddGrpcFluent(options =>
            {
                foreach (var type in types)
                {
                    options.Types.Add(type);
                }
            });
        }

        private static IServiceCollection InternalAddGrpcFluent(this IServiceCollection services)
        {
            return services
                .AddSingleton<IApplicationModelProvider, DefaultApplicationModelProvider>()
                .AddSingleton<ApplicationModelHolder, ApplicationModelHolder>()
                .AddSingleton<IMethodProvider, MethodProvider>()
                .AddSingleton<IServerServiceDefinitionProvider, ServerServiceDefinitionProvider>()
                .AddSingleton<IServerMethodInvokerFactory, DefaultServerMethodInvokerFactory>();
        }
    }
}