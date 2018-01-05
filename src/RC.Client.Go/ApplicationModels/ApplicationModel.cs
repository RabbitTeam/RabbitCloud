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

        private static ParameterModel CreateParameterModel(RequestModel requestModel, ParameterInfo parameter)
        {
            var parameterModel = new ParameterModel(parameter, parameter.GetCustomAttributes().ToArray())
            {
                ParameterName = parameter.Name,
                Request = requestModel
            };

            var name = parameterModel.ParameterName;

            var isPathVariable = requestModel.Path.Variables.Any(v => string.Equals(v, name, StringComparison.OrdinalIgnoreCase));

            var target = isPathVariable ? ParameterTarget.Path : ParameterTarget.Query;

            var parameterTargetProvider = parameterModel.Attributes.OfType<IParameterTargetProvider>().FirstOrDefault();
            if (parameterTargetProvider != null)
                target = parameterTargetProvider.Target;

            parameterModel.Target = target;

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

    public class ApplicationModel
    {
        public IList<ServiceModel> Services { get; } = new List<ServiceModel>();
    }

    public class ServiceModel
    {
        public ServiceModel(TypeInfo serviceType, IReadOnlyList<object> attributes)
        {
            if (attributes == null)
                throw new ArgumentNullException(nameof(attributes));

            Type = serviceType ?? throw new ArgumentNullException(nameof(serviceType));
            Attributes = new List<object>(attributes);
            Properties = new Dictionary<object, object>();
        }

        public TypeInfo Type { get; }
        public string Url { get; set; }
        public IReadOnlyList<object> Attributes { get; }
        public IDictionary<object, object> Properties { get; }
        public IList<RequestModel> Requests { get; } = new List<RequestModel>();
    }

    public struct RequestPath
    {
        public RequestPath(string pathTemplate)
        {
            PathTemplate = pathTemplate;
            Variables = TemplateEngine.GetVariables(pathTemplate);
        }

        public string PathTemplate { get; }
        public IReadOnlyList<string> Variables { get; }

        #region Overrides of ValueType

        /// <summary>Returns the fully qualified type name of this instance.</summary>
        /// <returns>The fully qualified type name.</returns>
        public override string ToString()
        {
            return PathTemplate;
        }

        #endregion Overrides of ValueType
    }

    public class RequestModel
    {
        public RequestModel(MethodInfo methodInfo, IReadOnlyList<object> attributes)
        {
            if (attributes == null)
                throw new ArgumentNullException(nameof(attributes));

            MethodInfo = methodInfo ?? throw new ArgumentNullException(nameof(methodInfo));
            Attributes = new List<object>(attributes);
            Properties = new Dictionary<object, object>();
            Parameters = new List<ParameterModel>();
        }

        public IReadOnlyList<object> Attributes { get; }
        public IDictionary<object, object> Properties { get; }
        public MethodInfo MethodInfo { get; }
        public IList<ParameterModel> Parameters { get; set; }
        public ServiceModel ServiceModel { get; set; }
        public RequestPath Path { get; set; }

        public Type RequesType { get; set; }
        public Type ResponseType { get; set; }
    }

    public class ParameterModel
    {
        public ParameterModel(ParameterInfo parameterInfo, IReadOnlyList<object> attributes)
        {
            if (attributes == null)
                throw new ArgumentNullException(nameof(attributes));

            ParameterInfo = parameterInfo ?? throw new ArgumentNullException(nameof(parameterInfo));
            Attributes = new List<object>(attributes);
            Properties = new Dictionary<object, object>();
        }

        public RequestModel Request { get; set; }
        public IReadOnlyList<object> Attributes { get; }
        public IDictionary<object, object> Properties { get; }
        public ParameterInfo ParameterInfo { get; }
        public string ParameterName { get; set; }
        public ParameterTarget Target { get; set; }
    }
}