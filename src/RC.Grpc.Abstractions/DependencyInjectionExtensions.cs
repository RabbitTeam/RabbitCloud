using Google.Protobuf;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyModel;
using Newtonsoft.Json;
using Rabbit.Cloud.Abstractions.Utilities;
using Rabbit.Cloud.Grpc.Abstractions.Internal;
using Rabbit.Cloud.Grpc.Abstractions.Method;
using Rabbit.Cloud.Grpc.Abstractions.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Rabbit.Cloud.Grpc.Abstractions
{
    internal static class MarshallerFactory
    {
        private static class Cache
        {
            private static readonly IDictionary<object, object> Caches = new Dictionary<object, object>();

            private static T GetCache<T>(object key, Func<T> factory)
            {
                if (Caches.TryGetValue(key, out var cache))
                {
                    return (T)cache;
                }
                return (T)(Caches[key] = factory());
            }

            public static Func<IMessage> GetInstanceFactory(Type type)
            {
                var key = ("InstanceFactory", type);
                return GetCache(key, () => Expression.Lambda<Func<IMessage>>(Expression.New(type)).Compile());
            }
        }

        public static object CreateMarshaller(Type type)
        {
            Func<object, byte[]> serializer;
            Func<byte[], object> deserializer;
            if (typeof(IMessage).IsAssignableFrom(type))
            {
                serializer = model => ((IMessage)model).ToByteArray();
                deserializer = data =>
                {
                    var message = Cache.GetInstanceFactory(type)();
                    message.MergeFrom(data);
                    return message;
                };
            }
            else
            {
                serializer = model => Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(model));
                deserializer = data => JsonConvert.DeserializeObject(Encoding.UTF8.GetString(data), type);
            }
            return MarshallerUtilities.CreateMarshaller(type, model => model == null ? null : serializer(model), data => data == null ? null : deserializer(data));
        }
    }

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
                    options.MarshallerFactory = MarshallerFactory.CreateMarshaller;
                    configure?.Invoke(options);
                })
                .AddSingleton<IMethodProvider, DefaultMethodProvider>()
                .AddSingleton<IMethodCollection>(s =>
                {
                    var providers = s.GetRequiredService<IEnumerable<IMethodProvider>>();
                    var methods = new MethodCollection();

                    foreach (var provider in providers)
                    {
                        provider.Collect(methods);
                    }
                    return methods;
                });
        }
    }
}