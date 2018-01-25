using Microsoft.Extensions.DependencyInjection;
using System;

namespace Rabbit.Go.Interceptors
{
    public class ServiceFilterAttribute : Attribute, IInterceptorFactory, IOrderedInterceptor
    {
        public ServiceFilterAttribute(Type type)
        {
            ServiceType = type ?? throw new ArgumentNullException(nameof(type));
        }

        public Type ServiceType { get; }

        #region Implementation of IInterceptorFactory

        public bool IsReusable { get; set; }

        public IInterceptorMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            var service = serviceProvider.GetRequiredService(ServiceType);

            if (!(service is IInterceptorMetadata filter))
            {
                throw new InvalidOperationException($"FormatFilterFactoryAttribute_TypeMustImplementIFilter {typeof(ServiceFilterAttribute).Name} {typeof(IInterceptorMetadata).Name}");
            }

            return filter;
        }

        #endregion Implementation of IInterceptorFactory

        #region Implementation of IOrderedInterceptor

        public int Order { get; set; }

        #endregion Implementation of IOrderedInterceptor
    }
}