using Rabbit.Cloud.Abstractions.Utilities;
using Rabbit.Cloud.Client.Go.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Client.Go.ApplicationModels
{
    public class RabbitApplicationBuilder
    {
        public static ApplicationModel BuildModel(IEnumerable<TypeInfo> types)
        {
            var applicationModel = new ApplicationModel();
            foreach (var typeInfo in types)
            {
                var serviceModel = CreateServiceModel(typeInfo);
                applicationModel.Services.Add(serviceModel);
            }

            return applicationModel;
        }

        private static ServiceModel CreateServiceModel(TypeInfo type)
        {
            var serviceModel = new ServiceModel(type, type.GetCustomAttributes().ToArray())
            {
                Url = GetBaseUrl(type)
            };
            foreach (var method in type.GetMethods())
            {
                var requestModel = CreateRequestModel(serviceModel, method);
                serviceModel.Requests.Add(requestModel);
            }
            return serviceModel;
        }

        private static RequestModel CreateRequestModel(ServiceModel serviceModel, MethodInfo method)
        {
            var requestModel = new RequestModel(method, method.GetCustomAttributes().ToArray())
            {
                Path = new RequestPath(GetPath(method)),
                ServiceModel = serviceModel,
                RequesType = GetRequestType(method),
                ResponseType = GetResponseType(method)
            };

            foreach (var parameter in method.GetParameters())
            {
                requestModel.Parameters.Add(CreateParameterModel(requestModel, parameter));
            }

            return requestModel;
        }

        private static Type GetRequestType(MethodBase method)
        {
            return method.GetParameters().LastOrDefault()?.ParameterType;
        }

        private static Type GetResponseType(MethodInfo method)
        {
            var returnType = method.ReturnType;
            if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
                return returnType.GenericTypeArguments[0];

            return returnType;
        }

        private static ParameterTarget GetParameterTarget(ParameterModel parameterModel)
        {
            var targetProvider = parameterModel.Attributes.OfType<IParameterTargetProvider>().FirstOrDefault();
            if (targetProvider != null)
                return targetProvider.Target;

            var name = parameterModel.ParameterName;
            var isPathVariable = parameterModel.Request.Path.Variables.Any(v => string.Equals(v, name, StringComparison.OrdinalIgnoreCase));

            if (isPathVariable)
                return ParameterTarget.Path;

            var typeCode = Type.GetTypeCode(parameterModel.ParameterInfo.ParameterType);

            return typeCode == TypeCode.Object ? ParameterTarget.Body : ParameterTarget.Query;
        }

        private static ParameterModel CreateParameterModel(RequestModel requestModel, ParameterInfo parameter)
        {
            var parameterModel = new ParameterModel(parameter, parameter.GetCustomAttributes().ToArray())
            {
                ParameterName = parameter.Name,
                Request = requestModel
            };

            parameterModel.Target = GetParameterTarget(parameterModel);

            return parameterModel;
        }

        private static string GetBaseUrl(MemberInfo proxyType)
        {
            var goClientProvider = proxyType.GetTypeAttribute<IClientProvider>();
            if (goClientProvider != null && !string.IsNullOrEmpty(goClientProvider.Url))
                return goClientProvider.Url;

            var name = proxyType.Name;
            if (name.StartsWith("I"))
            {
                name = name.Substring(1);
            }
            if (name.EndsWith("Service"))
            {
                name = name.Substring(0, name.Length - 7);
            }
            else if (name.EndsWith("Services"))
            {
                name = name.Substring(0, name.Length - 8);
            }

            return "http://" + name;
        }

        private static string GetPath(MemberInfo method)
        {
            var pathProvider = method.GetTypeAttribute<IPathProvider>();
            if (pathProvider != null && !string.IsNullOrEmpty(pathProvider.Path))
                return pathProvider.Path;

            var name = method.Name;
            if (name.EndsWith("Async"))
                name = name.Substring(0, name.Length - 5);

            return name;
        }
    }
}