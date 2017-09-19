using Rabbit.Cloud.Facade.Abstractions;
using Rabbit.Cloud.Facade.Abstractions.Filters;
using Rabbit.Cloud.Facade.Abstractions.MessageBuilding;
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
                HttpMethod = requestMappingAttribute.Method,
                ServiceRouteInfo = new ServiceRouteInfo
                {
                    Template = (facadeClientAttribute.Url ?? facadeClientAttribute.Name).TrimEnd('/') + "/" + request.RouteUrl.TrimStart('/')
                },
                DisplayName = request.RouteUrl,
                FilterDescriptors = request.Filters.Select(i => new FilterDescriptor(i)).ToArray(),
                Parameters = request.Parameters.Select(CreateParameterDescriptor).ToArray()
            };

            AddDefaultHeaders(serviceDescriptor, attributes.OfType<ToHeaderAttribute>());
            AddDefaultQuerys(serviceDescriptor, attributes.OfType<ToQueryAttribute>());

            return serviceDescriptor;
        }

        private static void AddDefaultHeaders(ServiceDescriptor service, IEnumerable<ToHeaderAttribute> toHeaderAttributes)
        {
            var headers = toHeaderAttributes.GroupBy(i => i.Name).Select(i => new KeyValuePair<string, IEnumerable<string>>(i.Key, i.Select(z => z.Value?.ToString()))).ToArray();
            service.Headers = service.Headers == null ? headers : service.Headers.Concat(headers);
        }

        private static void AddDefaultQuerys(ServiceDescriptor service, IEnumerable<ToQueryAttribute> toQueryAttributes)
        {
            var querys = toQueryAttributes.GroupBy(i => i.Name).Select(i => new KeyValuePair<string, IEnumerable<string>>(i.Key, i.Select(z => z.Value?.ToString()))).ToArray();
            service.Querys = service.Querys == null ? querys : service.Querys.Concat(querys);
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
                ParameterType = model.ParameterInfo.ParameterType,
                BuildingInfo = BuildingInfo.GetBuildingInfo(model.Attributes)
            };
            return descriptor;
        }

        #endregion Private Method
    }
}