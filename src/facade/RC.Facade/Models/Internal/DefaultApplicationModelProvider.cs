using Rabbit.Cloud.Facade.Abstractions;
using Rabbit.Cloud.Facade.Abstractions.Filters;
using Rabbit.Cloud.Facade.Abstractions.MessageBuilding;
using Rabbit.Cloud.Facade.Abstractions.Utilities.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;

namespace Rabbit.Cloud.Facade.Models.Internal
{
    public class DefaultApplicationModelProvider : IApplicationModelProvider
    {
        #region Implementation of IApplicationModelProvider

        public int Order { get; } = -1000;

        public void OnProvidersExecuting(ApplicationModelProviderContext context)
        {
            var applicationModel = context.Result;
            var serviceModels = GetServiceModels(context.ServiceTypes);

            foreach (var serviceModel in serviceModels)
            {
                serviceModel.Application = applicationModel;
                applicationModel.Services.Add(serviceModel);
            }
        }

        public void OnProvidersExecuted(ApplicationModelProviderContext context)
        {
        }

        #endregion Implementation of IApplicationModelProvider

        #region Private Method

        private static IEnumerable<ServiceModel> GetServiceModels(IEnumerable<TypeInfo> serviceTypeInfos)
        {
            return serviceTypeInfos.Select(GetServiceModel);
        }

        private static ServiceModel GetServiceModel(TypeInfo serviceTypeInfo)
        {
            var serviceModel = new ServiceModel(serviceTypeInfo, serviceTypeInfo.GetCustomAttributes(false));
            AddFilters(serviceModel.Filters, serviceModel.Attributes);
            EnsureFacadeClientAttribute(serviceModel);

            foreach (var requestModel in GetRequestModels(serviceTypeInfo))
            {
                requestModel.Service = serviceModel;
                serviceModel.Requests.Add(requestModel);
            }

            return serviceModel;
        }

        private static void EnsureFacadeClientAttribute(ServiceModel serviceModel)
        {
            var facadeClientAttribute = serviceModel.Attributes.OfType<FacadeClientAttribute>().LastOrDefault();
            if (facadeClientAttribute == null)
                return;

            if (string.IsNullOrEmpty(facadeClientAttribute.Name) &&
                string.IsNullOrEmpty(facadeClientAttribute.Url))
            {
                var typeName = serviceModel.ServiceType.Name;
                if (typeName.StartsWith("I"))
                    typeName = typeName.Remove(0, 1);
                facadeClientAttribute.Name = typeName;
            }

            serviceModel.ServiceName = facadeClientAttribute.Name;
        }

        private static IEnumerable<RequestModel> GetRequestModels(Type serviceTypeInfo)
        {
            return serviceTypeInfo.GetMethods().Select(GetRequestModel).Where(i => i != null);
        }

        private static RequestModel GetRequestModel(MethodInfo methodInfo)
        {
            var requestModel = new RequestModel(methodInfo, methodInfo.GetCustomAttributes(false));
            AddFilters(requestModel.Filters, requestModel.Attributes);

            var requestMappingAttribute = requestModel.Attributes.OfType<RequestMappingAttribute>().LastOrDefault();
            if (requestMappingAttribute == null)
                return null;

            if (string.IsNullOrEmpty(requestMappingAttribute.Value))
            {
                var methodName = methodInfo.Name;
                if (methodName.EndsWith("Async", StringComparison.OrdinalIgnoreCase))
                {
                    methodName = methodName.Substring(0, methodName.Length - 5);
                }
                requestMappingAttribute.Value = methodName;
            }

            requestModel.RouteUrl = requestMappingAttribute.Value;

            foreach (var parameterModel in GetParameterModels(methodInfo, requestMappingAttribute.Method))
            {
                parameterModel.Request = requestModel;
                requestModel.Parameters.Add(parameterModel);
            }

            return requestModel;
        }

        private static IEnumerable<ParameterModel> GetParameterModels(MethodBase methodInfo, HttpMethod httpMethod)
        {
            return methodInfo.GetParameters().Select(i => GetParameterModel(i, httpMethod));
        }

        private static ParameterModel GetParameterModel(ParameterInfo parameterInfo, HttpMethod httpMethod)
        {
            var attributes = parameterInfo.GetCustomAttributes(false).ToList();

            var buildingTargetMetadata = attributes.OfType<IBuilderTargetMetadata>().LastOrDefault();
            if (buildingTargetMetadata == null)
            {
                var haveBody = httpMethod.HaveBody();
                var isForm = haveBody;

                if (isForm)
                    isForm = Type.GetTypeCode(parameterInfo.ParameterType) == TypeCode.Object;

                if (isForm)
                {
                    attributes.Add(new ToFormAttribute(parameterInfo.Name));
                }
                else
                {
                    attributes.Add(new ToQueryAttribute(parameterInfo.Name));
                }
            }

            return new ParameterModel(parameterInfo, attributes);
        }

        private static void AddFilters(ICollection<IFilterMetadata> filters, IEnumerable attributes)
        {
            foreach (var filter in attributes.OfType<IFilterMetadata>())
            {
                filters.Add(filter);
            }
        }

        #endregion Private Method
    }
}