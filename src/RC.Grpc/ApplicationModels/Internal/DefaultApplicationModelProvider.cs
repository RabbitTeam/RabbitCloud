using Google.Protobuf;
using Microsoft.Extensions.Options;
using Rabbit.Cloud.Abstractions.Serialization;
using Rabbit.Cloud.Abstractions.Utilities;
using Rabbit.Cloud.ApplicationModels;
using Rabbit.Cloud.Grpc.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Rabbit.Cloud.Grpc.ApplicationModels.Internal
{
    public class SerializerCacheTable
    {
        private readonly IEnumerable<ISerializer> _serializers;

        public SerializerCacheTable(IOptions<RabbitCloudOptions> options)
        {
            _serializers = options.Value.Serializers;
        }

        private static readonly ConcurrentDictionary<Type, ISerializer> SerializerCaches = new ConcurrentDictionary<Type, ISerializer>();

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

                //                var isServerService = type.GetTypeAttribute<GrpcServiceAttribute>() != null;
                var typeAttributes = type.GetCustomAttributes(false);
                var serviceModel = new ServiceModel(type, typeAttributes)
                {
                    ServiceName = serviceName
                };
                applicationModel.Services.Add(serviceModel);

                /*ServerServiceModel serverService = null;
                if (isServerService)
                {
                    serverService = new ServerServiceModel(type, typeAttributes)
                    {
                        ServiceName = serviceName
                    };
                    applicationModel.ServerServices.Add(serverService);
                }*/

                foreach (var methodInfo in GetMethodInfos(type))
                {
                    var methodModel = CreateMethodModel(serviceModel, methodInfo);
                    serviceModel.Methods.Add(methodModel);

                    /*                    if (!isServerService)
                                            continue;

                                        var serverMethod = new ServerMethodModel(methodInfo, methodInfo.GetCustomAttributes(false))
                                        {
                                            Method = methodModel,
                                            ServerService = serverService
                                        };

                                        serverService.ServerMethods.Add(serverMethod);*/
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
            //todo: 考虑动态实现
            return new CodecModel(marshallerType.GetTypeInfo(), marshallerType.GetCustomAttributes(false))
            {
                Deserializer = data => Deserialize(marshallerType, data),
                MethodModel = methodModel,
                Serializer = Serialize
            };
        }

        private byte[] Serialize(object instance)
        {
            switch (instance)
            {
                case null:
                    return null;

                case IMessage message:
                    return message.ToByteArray();
            }

            var serializer = _serializerTable.GetRequiredSerializer(instance.GetType());
            return serializer.Serialize(instance);
        }

        private object Deserialize(Type type, byte[] data)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));
            if (data == null)
                return null;

            if (typeof(IMessage).IsAssignableFrom(type))
            {
                //todo: optimization create instance
                var message = (IMessage)Activator.CreateInstance(type);
                message.MergeFrom(data);
                return message;
            }

            var serializer = _serializerTable.GetRequiredSerializer(type);
            return serializer.Deserialize(type, data);
        }

        #endregion Private Method
    }
}