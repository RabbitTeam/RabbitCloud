using Rabbit.Cloud.Facade.Abstractions;
using Rabbit.Cloud.Facade.Abstractions.Abstractions;
using Rabbit.Cloud.Facade.Abstractions.Filters;
using Rabbit.Cloud.Facade.Abstractions.ModelBinding;
using Rabbit.Cloud.Facade.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;

namespace Rabbit.Cloud.Facade.Internal
{
    public class ApplicationServiceDescriptorProvider : IServiceDescriptorProvider
    {
        private readonly IApplicationModelProvider[] _applicationModelProviders;

        public ApplicationServiceDescriptorProvider(IEnumerable<IApplicationModelProvider> applicationModelProviders)
        {
            if (applicationModelProviders == null)
                throw new ArgumentNullException(nameof(applicationModelProviders));

            _applicationModelProviders = applicationModelProviders.OrderBy(i => i.Order).ToArray();
        }

        #region Implementation of IServiceDescriptorProvider

        public int Order { get; } = -1000;

        public void OnProvidersExecuting(ServiceDescriptorProviderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            foreach (var descriptor in GetDescriptors())
            {
                context.Results.Add(descriptor);
            }
        }

        public void OnProvidersExecuted(ServiceDescriptorProviderContext context)
        {
            var keys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var action in context.Results)
            {
                foreach (var key in action.RouteValues.Keys)
                {
                    keys.Add(key);
                }
            }

            foreach (var action in context.Results)
            {
                foreach (var key in keys)
                {
                    if (!action.RouteValues.ContainsKey(key))
                    {
                        action.RouteValues.Add(key, null);
                    }
                }
            }
        }

        #endregion Implementation of IServiceDescriptorProvider

        #region Private Method

        private IEnumerable<ServiceDescriptor> GetDescriptors()
        {
            var applicationModel = BuildModel();
            foreach (var serviceModel in applicationModel.Services)
            {
                var facadeClientAttribute = serviceModel.Attributes.OfType<FacadeClientAttribute>().LastOrDefault();
                foreach (var requestModel in serviceModel.Requests)
                {
                    var requestMappingAttribute = requestModel.Attributes.OfType<RequestMappingAttribute>().LastOrDefault();
                    var serviceDescriptor = new ServiceDescriptor(requestModel.Method.GetHashCode().ToString())
                    {
                        HttpMethod = requestMappingAttribute?.Method == null ? HttpMethod.Get : new HttpMethod(requestMappingAttribute.Method),
                        BaseUrl = facadeClientAttribute.Name ?? facadeClientAttribute.Url,
                        AttributeRouteInfo = new AttributeRouteInfo
                        {
                            Template = requestModel.RequestUrl
                        },
                        DisplayName = requestModel.RequestUrl,
                        FilterDescriptors = requestModel.Filters.Select(i => new FilterDescriptor(i)).ToArray(),
                        Parameters = requestModel.Parameters.Select(CreateParameterDescriptor).ToArray()
                    };
                    yield return serviceDescriptor;
                }
            }
        }

        private static ParameterDescriptor CreateParameterDescriptor(ParameterModel model)
        {
            var descriptor = new ParameterDescriptor
            {
                Name = model.ParameterName,
                ParameterType = model.ParameterInfo.ParameterType,
                BindingInfo = BindingInfo.GetBindingInfo(model.ParameterInfo.GetCustomAttributes(false))
            };
            return descriptor;
        }

        private ApplicationModel BuildModel()
        {
            var facadeTypes = GetFacadeTypes();
            var context = new ApplicationModelProviderContext(facadeTypes);

            foreach (var provider in _applicationModelProviders)
            {
                provider.OnProvidersExecuting(context);
            }

            for (var i = _applicationModelProviders.Length - 1; i >= 0; i--)
            {
                _applicationModelProviders[i].OnProvidersExecuted(context);
            }

            return context.Result;
        }

        private static IEnumerable<TypeInfo> GetFacadeTypes()
        {
            return AppDomain.CurrentDomain.GetAssemblies().Where(i => !i.IsDynamic).SelectMany(i => i.ExportedTypes).Where(i =>
                  i.IsInterface && i.IsPublic && i.GetCustomAttribute<FacadeClientAttribute>() != null).Select(i => i.GetTypeInfo());
        }

        #endregion Private Method
    }
}