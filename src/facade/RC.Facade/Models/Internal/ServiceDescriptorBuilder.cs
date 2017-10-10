using Rabbit.Cloud.Facade.Abstractions;
using Rabbit.Cloud.Facade.Abstractions.Filters;
using System.Collections.Generic;
using System.Linq;

namespace Rabbit.Cloud.Facade.Models.Internal
{
    public static class ServiceDescriptorBuilder
    {
        public static IList<ServiceDescriptor> Build(ApplicationModel application)
        {
            IList<ServiceDescriptor> descriptors = new List<ServiceDescriptor>();
            foreach (var serviceModel in application.Services)
            {
                foreach (var requestModel in serviceModel.Requests)
                {
                    descriptors.Add(CreateServiceDescriptor(requestModel));
                }
            }
            return descriptors;
        }

        #region Private Method

        private static ServiceDescriptor CreateServiceDescriptor(RequestModel request)
        {
            var attributes = GetServiceAndRequestAttributes(request).ToArray();
            var facadeClientAttribute = attributes.OfType<FacadeClientAttribute>().LastOrDefault();
            var requestMappingAttribute = attributes.OfType<RequestMappingAttribute>().LastOrDefault();

            var serviceDescriptor = new ServiceDescriptor(request.Method.GetHashCode().ToString())
            {
                HttpMethod = requestMappingAttribute.Method.Method,
                ServiceRouteInfo = new ServiceRouteInfo
                {
                    Template = (facadeClientAttribute.Url ?? facadeClientAttribute.Name).TrimEnd('/') + "/" + request.RouteUrl.TrimStart('/')
                },
                DisplayName = request.RouteUrl,
                FilterDescriptors = request.Service.Filters.Concat(request.Filters).Select(i => new FilterDescriptor(i)).ToArray(),
                Parameters = request.Parameters.Select(CreateParameterDescriptor).ToArray()
            };

            return serviceDescriptor;
        }

        private static IEnumerable<object> GetServiceAndRequestAttributes(RequestModel request)
        {
            if (request.Service.Attributes != null)
                foreach (var attribute in request.Service.Attributes)
                {
                    yield return attribute;
                }
            if (request.Attributes != null)
                foreach (var attribute in request.Attributes)
                {
                    yield return attribute;
                }
        }

        private static ParameterDescriptor CreateParameterDescriptor(ParameterModel model)
        {
            var descriptor = new ParameterDescriptor
            {
                Name = model.ParameterName,
                ParameterType = model.ParameterInfo?.ParameterType,
                BuildingInfo = model.BuildingInfo
            };
            return descriptor;
        }

        #endregion Private Method
    }
}