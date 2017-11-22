using Google.Protobuf;
using Newtonsoft.Json;
using Rabbit.Cloud.Abstractions.Utilities;
using Rabbit.Cloud.Grpc.Fluent.ApplicationModels;
using Rabbit.Cloud.Grpc.Fluent.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Rabbit.Cloud.Grpc.Fluent.Internal
{
    public class DefaultApplicationModelProvider : IApplicationModelProvider
    {
        #region Implementation of IApplicationModelProvider

        public int Order { get; } = 10;

        public void OnProvidersExecuting(ApplicationModelProviderContext context)
        {
            var applicationModel = context.Result;
            foreach (var type in context.Types)
            {
                var serviceName = FluentUtilities.GetServiceName(type);

                var isServerService = type.GetTypeAttribute<GrpcServiceAttribute>() != null;
                var typeAttributes = type.GetCustomAttributes(false);
                var serviceModel = new ServiceModel(type, typeAttributes)
                {
                    ServiceName = serviceName
                };
                applicationModel.Services.Add(serviceModel);

                ServerServiceModel serverService = null;
                if (isServerService)
                {
                    serverService = new ServerServiceModel(type, typeAttributes)
                    {
                        ServiceName = serviceName
                    };
                    applicationModel.ServerServices.Add(serverService);
                }

                foreach (var methodInfo in GetMethodInfos(type))
                {
                    var methodModel = CreateMethodModel(serviceModel, methodInfo);
                    serviceModel.Methods.Add(methodModel);

                    if (!isServerService)
                        continue;

                    var serverMethod = new ServerMethodModel(methodInfo, methodInfo.GetCustomAttributes(false))
                    {
                        Method = methodModel,
                        ServerService = serverService
                    };

                    serverService.ServerMethods.Add(serverMethod);
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
            return methods.Where(i => i.IsPublic && i.GetParameters().Any() && i.ReturnType != typeof(void) && i.GetTypeAttribute<IGrpcIgnoreProvider>() == null);
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

        private static MethodModel CreateMethodModel(ServiceModel serviceModel, MethodInfo methodInfo)
        {
            var methodName = FluentUtilities.GetMethodName(methodInfo);
            var methodModel = new MethodModel(methodInfo, methodInfo.GetCustomAttributes(false))
            {
                Name = methodName,
                RequestMarshaller = null,
                ResponseMarshaller = null,
                ServiceModel = serviceModel,
                Type = FluentUtilities.GetMethodType(methodInfo)
            };

            var requestMarshallerModel = CreateMarshallerModel(methodModel, FluentUtilities.GetRequestType(methodInfo));
            var responseMarshallerModel = CreateMarshallerModel(methodModel, FluentUtilities.GetResponseType(methodInfo));

            methodModel.RequestMarshaller = requestMarshallerModel;
            methodModel.ResponseMarshaller = responseMarshallerModel;

            return methodModel;
        }

        private static MarshallerModel CreateMarshallerModel(MethodModel methodModel, Type marshallerType)
        {
            //todo: 考虑动态实现
            return new MarshallerModel(marshallerType.GetTypeInfo(), marshallerType.GetCustomAttributes(false))
            {
                Deserializer = data =>
                {
                    if (data == null)
                        return null;
                    if (!typeof(IMessage).IsAssignableFrom(marshallerType))
                        return JsonConvert.DeserializeObject(Encoding.UTF8.GetString(data), marshallerType);
                    var message = (IMessage)Activator.CreateInstance(marshallerType);
                    message.MergeFrom(data);
                    return message;
                },
                MethodModel = methodModel,
                Serializer = model =>
                {
                    switch (model)
                    {
                        case null:
                            return null;

                        case IMessage message:
                            return message.ToByteArray();
                    }
                    return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(model));
                }
            };
        }

        #endregion Private Method
    }
}