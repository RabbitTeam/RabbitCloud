using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using Rabbit.Cloud.Abstractions.Utilities;
using Rabbit.Cloud.Grpc.Abstractions;
using Rabbit.Cloud.Grpc.Server.Internal;
using System;
using System.Linq;
using System.Reflection;

namespace Rabbit.Cloud.Grpc.Server
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddGrpcServer(this IServiceCollection services, Func<AssemblyName, bool> assemblyPredicate = null, Func<MethodInfo, bool> methodPredicate = null, Func<Type, bool> typePredicate = null)
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
                .AddGrpcServer(options =>
                {
                    options.Entries = entries;
                });
        }

        public static IServiceCollection AddGrpcServer(this IServiceCollection services, Action<ServerServiceDefinitionProviderOptions> configure)
        {
            return services
                .Configure<ServerServiceDefinitionProviderOptions>(options =>
               {
                   configure?.Invoke(options);
                   if (options.Factory == null)
                       options.Factory = (sp, serviceType) =>
                       {
                           if (serviceType == null)
                               throw new ArgumentNullException(nameof(serviceType));

                           var instance = sp?.GetService(serviceType);
                           if (instance == null && serviceType.IsClass && !serviceType.IsAbstract)
                               instance = Activator.CreateInstance(serviceType);

                           if (instance == null)
                               throw new NotImplementedException($"Unable to parse the service type '{serviceType.FullName}'");

                           return instance;
                       };
               })
                .AddSingleton<IServerServiceDefinitionProvider, DefaultServerServiceDefinitionProvider>()
                .AddSingleton<IServiceDefinitionCollection, ServiceDefinitionCollection>();
        }
    }
}