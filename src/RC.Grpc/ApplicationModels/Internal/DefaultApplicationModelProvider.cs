using Google.Protobuf;
using Microsoft.Extensions.Options;
using Rabbit.Cloud.Abstractions.Serialization;
using Rabbit.Cloud.Abstractions.Utilities;
using Rabbit.Cloud.ApplicationModels;
using Rabbit.Cloud.Grpc.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Rabbit.Cloud.Grpc.ApplicationModels.Internal
{
    public class SerializerCacheTable
    {
        private readonly IEnumerable<ISerializer> _serializers;
        private static readonly ConcurrentDictionary<Type, ISerializer> SerializerCaches = new ConcurrentDictionary<Type, ISerializer>();

        public SerializerCacheTable(IOptions<RabbitCloudOptions> options)
        {
            _serializers = new[] { MessageSerializer.Instance }.Concat(options.Value.Serializers).ToArray();
        }

        public ISerializer GetRequiredSerializer(Type type)
        {
            if (!SerializerCaches.TryGetValue(type, out var serializer))
            {
                serializer = _serializers.FindAvailableSerializer(type);
                if (serializer != null)
                    SerializerCaches[type] = serializer;
            }

            if (serializer == null)
                throw RpcExceptionUtilities.NotFoundSerializer(type);

            return serializer;
        }
    }

    internal class MessageSerializer : ISerializer
    {
        public static MessageSerializer Instance { get; } = new MessageSerializer();

        #region Implementation of ISerializer

        public bool CanHandle(Type type)
        {
            return typeof(IMessage).IsAssignableFrom(type);
        }

        public void Serialize(Stream stream, object instance)
        {
            var message = (IMessage)instance;
            message.WriteTo(stream);
        }

        public object Deserialize(Type type, Stream stream)
        {
            var message = Cache.CreateMessage(type);
            message.MergeFrom(stream);
            return message;
        }

        #endregion Implementation of ISerializer

        private static class Cache
        {
            private static readonly ConcurrentDictionary<Type, Func<IMessage>> FactoryCaches = new ConcurrentDictionary<Type, Func<IMessage>>();

            public static IMessage CreateMessage(Type type)
            {
                if (FactoryCaches.TryGetValue(type, out var factory))
                    return factory();

                factory = Expression.Lambda<Func<IMessage>>(Expression.Convert(Expression.New(type), typeof(IMessage))).Compile();
                FactoryCaches.TryAdd(type, factory);

                return factory();
            }
        }
    }

    public class DefaultApplicationModelProvider : IApplicationModelProvider
    {
        private readonly SerializerCacheTable _serializerTable;

        public DefaultApplicationModelProvider(SerializerCacheTable serializerTable)
        {
            _serializerTable = serializerTable;
        }

        #region Implementation of IApplicationModelProvider

        public int Order { get; } = 10;

        public void OnProvidersExecuting(ApplicationModelProviderContext context)
        {
            var applicationModel = context.Result;
            foreach (var type in context.Types)
            {
                var serviceName = FluentUtilities.GetServiceName(type);

                var typeAttributes = type.GetCustomAttributes(false);
                var serviceModel = new ServiceModel(type, typeAttributes)
                {
                    ServiceName = serviceName
                };
                applicationModel.Services.Add(serviceModel);

                foreach (var methodInfo in GetMethodInfos(type))
                {
                    var methodModel = CreateMethodModel(serviceModel, methodInfo);
                    serviceModel.Methods.Add(methodModel);
                }
            }
        }

        public void OnProvidersExecuted(ApplicationModelProviderContext context)
        {
        }

        #endregion Implementation of IApplicationModelProvider

        #region Private Method

        private static IEnumerable<MethodInfo> GetMethodInfos(Type type)
        {
            var methods = new List<MethodInfo>();
            GetMethodInfos(type, methods);
            return methods.Where(i => i.IsPublic && i.GetParameters().Any() && i.ReturnType != typeof(void) && i.GetTypeAttribute<IServiceIgnoreProvider>() == null);
        }

        private static void GetMethodInfos(Type type, List<MethodInfo> methods)
        {
            if (!type.IsInterface)
            {
                methods.AddRange(type.GetMethods().Where(i => i.DeclaringType != typeof(object)));
            }
            else
            {
                while (true)
                {
                    if (type.IsInterface)
                    {
                        methods.AddRange(type.GetMethods());
                        foreach (var @interface in type.GetInterfaces())
                        {
                            GetMethodInfos(@interface, methods);
                        }
                    }
                    break;
                }
            }
        }

        private MethodModel CreateMethodModel(ServiceModel serviceModel, MethodInfo methodInfo)
        {
            var methodName = FluentUtilities.GetMethodName(methodInfo);
            var methodModel = new MethodModel(methodInfo, methodInfo.GetCustomAttributes(false))
            {
                Name = methodName,
                ServiceModel = serviceModel
            };

            var requestMarshallerModel = CreateMarshallerModel(methodModel, FluentUtilities.GetRequestType(methodInfo));
            var responseMarshallerModel = CreateMarshallerModel(methodModel, FluentUtilities.GetResponseType(methodInfo));

            methodModel.RequestCodec = requestMarshallerModel;
            methodModel.ResponseCodec = responseMarshallerModel;

            return methodModel;
        }

        private CodecModel CreateMarshallerModel(MethodModel methodModel, Type marshallerType)
        {
            var serializer = GetSerializer(marshallerType);
            return new CodecModel(marshallerType.GetTypeInfo(), marshallerType.GetCustomAttributes(false))
            {
                Deserializer = data => serializer.Deserialize(marshallerType, data),
                MethodModel = methodModel,
                Serializer = serializer.Serialize
            };
        }

        private ISerializer GetSerializer(Type type)
        {
            if (type == null)
                return null;
            return typeof(IMessage).IsAssignableFrom(type) ? MessageSerializer.Instance : _serializerTable.GetRequiredSerializer(type);
        }

        #endregion Private Method
    }
}