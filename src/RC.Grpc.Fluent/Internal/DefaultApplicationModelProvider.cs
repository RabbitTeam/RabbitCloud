using Newtonsoft.Json;
using Rabbit.Cloud.Grpc.Fluent.ApplicationModels;
using Rabbit.Cloud.Grpc.Fluent.Utilities;
using System;
using System.Linq;
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

                var serviceModel = new ServiceModel { ServiceName = serviceName };
                applicationModel.Services.Add(serviceModel);

                var methods = type.IsInterface ? type.GetInterfaces().SelectMany(i => i.GetMethods()).ToArray() : type.GetMethods().Where(i => i.DeclaringType != typeof(object)).ToArray();

                foreach (var methodInfo in methods)
                {
                    var methodName = FluentUtilities.GetMethodName(methodInfo);
                    var methodModel = new MethodModel
                    {
                        Name = methodName,
                        RequestMarshaller = null,
                        ResponseMarshaller = null,
                        ServiceModel = serviceModel,
                        Type = FluentUtilities.GetMethodType(methodInfo)
                    };

                    MarshallerModel CreateMarshallerModel(Type marshallerType)
                    {
                        return new MarshallerModel
                        {
                            Deserializer = data => JsonConvert.DeserializeObject(Encoding.UTF8.GetString(data), marshallerType),
                            MethodModel = methodModel,
                            Serializer = model => Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(model)),
                            Type = marshallerType
                        };
                    }

                    var requestMarshallerModel = CreateMarshallerModel(FluentUtilities.GetRequestType(methodInfo));
                    var responseMarshallerModel = CreateMarshallerModel(FluentUtilities.GetResponseType(methodInfo));

                    methodModel.RequestMarshaller = requestMarshallerModel;
                    methodModel.ResponseMarshaller = responseMarshallerModel;

                    serviceModel.Methods.Add(methodModel);
                }
            }
        }

        public void OnProvidersExecuted(ApplicationModelProviderContext context)
        {
        }

        #endregion Implementation of IApplicationModelProvider
    }
}