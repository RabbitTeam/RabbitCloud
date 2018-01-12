using Microsoft.Extensions.Options;
using Rabbit.Cloud.Abstractions.Utilities;
using Rabbit.Cloud.Client.Go.Abstractions;
using Rabbit.Cloud.Client.Go.Abstractions.Filters;
using Rabbit.Cloud.Client.Go.ApplicationModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Rabbit.Cloud.Client.Go.Internal
{
    public class DefaultApplicationModelProvider : IApplicationModelProvider
    {
        private readonly ICollection<IFilterMetadata> _globalFilters;

        public DefaultApplicationModelProvider(IOptions<GoOptions> goOptionsAccessor)
        {
            _globalFilters = goOptionsAccessor.Value.Filters;
        }

        #region Implementation of IApplicationModelProvider

        public int Order => -1000;

        public void OnProvidersExecuting(ApplicationModelProviderContext context)
        {
            var application = context.Result;

            foreach (var filter in _globalFilters)
                application.Filters.Add(filter);

            foreach (var typeInfo in context.ServiceTypes)
            {
                var serviceModel = CreateServiceModel(typeInfo);
                serviceModel.Application = application;
                application.Services.Add(serviceModel);
            }
        }

        public virtual void OnProvidersExecuted(ApplicationModelProviderContext context)
        {
        }

        #endregion Implementation of IApplicationModelProvider

        #region Private Method

        private static void AddRange<T>(ICollection<T> list, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                list.Add(item);
            }
        }

        private static ServiceModel CreateServiceModel(TypeInfo type)
        {
            var serviceModel = new ServiceModel(type, type.GetCustomAttributes().ToArray())
            {
                Url = GetBaseUrl(type)
            };

            AddRange(serviceModel.Filters, serviceModel.Attributes.OfType<IFilterMetadata>());

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

            AddRange(requestModel.Filters, requestModel.Attributes.OfType<IFilterMetadata>());

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

            return isPathVariable ? ParameterTarget.Path : ParameterTarget.Query;
        }

        private static string GetParameterName(IEnumerable<object> attributes, ParameterInfo parameter)
        {
            var targetProvider = attributes.OfType<IParameterTargetProvider>().FirstOrDefault();
            return targetProvider?.Name ?? parameter.Name;
        }

        private static ParameterModel CreateParameterModel(RequestModel requestModel, ParameterInfo parameter)
        {
            var attributes = parameter.GetCustomAttributes().ToArray();
            var parameterModel = new ParameterModel(parameter, attributes)
            {
                ParameterName = GetParameterName(attributes, parameter),
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

        #endregion Private Method
    }
}