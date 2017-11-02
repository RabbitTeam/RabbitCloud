using Google.Protobuf;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Rabbit.Cloud.Grpc.Abstractions.Internal;
using Rabbit.Cloud.Grpc.Abstractions.Method;
using Rabbit.Cloud.Grpc.Abstractions.Utilities;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
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
        public static IServiceCollection AddGrpcCore(this IServiceCollection services, params Type[] types)
        {
            if (types == null)
                throw new ArgumentNullException(nameof(types));

            return services
                .Configure<DefaultMethodProviderOptions>(options =>
                {
                    options.Types = types;
                    options.MarshallerFactory = MarshallerFactory.CreateMarshaller;
                })
                .AddSingleton<IMethodCollection, MethodCollection>()
                .AddSingleton<IMethodProvider, DefaultMethodProvider>();
        }

        public static IServiceCollection AddGrpcCore(this IServiceCollection services, Action<DefaultMethodProviderOptions> configure)
        {
            return services
                .AddGrpcCore()
                .Configure(configure);
        }
    }
}