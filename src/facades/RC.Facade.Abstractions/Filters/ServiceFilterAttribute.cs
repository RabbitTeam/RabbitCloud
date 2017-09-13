using Microsoft.Extensions.DependencyInjection;
using System;

namespace Rabbit.Cloud.Facade.Abstractions.Filters
{
    public class ServiceFilterAttribute : Attribute, IFilterFactory
    {
        public ServiceFilterAttribute(Type type)
        {
            ServiceType = type ?? throw new ArgumentNullException(nameof(type));
        }

        public Type ServiceType { get; }

        public int Order { get; set; }

        #region Implementation of IFilterFactory

        public bool IsReusable { get; set; }

        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            var service = serviceProvider.GetRequiredService(ServiceType);

            if (!(service is IFilterMetadata filter))
            {
                throw new InvalidOperationException($"FormatFilterFactoryAttribute_TypeMustImplementIFilter({typeof(ServiceFilterAttribute).Name},{typeof(IFilterMetadata).Name})");
            }

            return filter;
        }

        #endregion Implementation of IFilterFactory
    }
}