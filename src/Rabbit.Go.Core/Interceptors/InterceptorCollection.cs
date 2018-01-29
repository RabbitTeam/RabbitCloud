using Rabbit.Go.Interceptors;
using System;
using System.Collections.ObjectModel;

namespace Rabbit.Go.Core.Interceptors
{
    public class InterceptorCollection : Collection<IInterceptorMetadata>
    {
        public IInterceptorMetadata Add<TInterceptorType>() where TInterceptorType : IInterceptorMetadata
        {
            return Add(typeof(TInterceptorType));
        }

        public IInterceptorMetadata Add(Type interceptorType)
        {
            if (interceptorType == null)
            {
                throw new ArgumentNullException(nameof(interceptorType));
            }

            return Add(interceptorType, 0);
        }

        public IInterceptorMetadata Add<TInterceptorType>(int order) where TInterceptorType : IInterceptorMetadata
        {
            return Add(typeof(TInterceptorType), order);
        }

        public IInterceptorMetadata Add(Type interceptorType, int order)
        {
            if (interceptorType == null)
            {
                throw new ArgumentNullException(nameof(interceptorType));
            }

            if (!typeof(IInterceptorMetadata).IsAssignableFrom(interceptorType))
            {
                var message = $"FormatTypeMustDeriveFromType {interceptorType.FullName} {typeof(IInterceptorMetadata).FullName}";
                throw new ArgumentException(message, nameof(interceptorType));
            }

            var filter = new TypeFilterAttribute(interceptorType) { Order = order };
            Add(filter);
            return filter;
        }

        public IInterceptorMetadata AddService<TInterceptorType>() where TInterceptorType : IInterceptorMetadata
        {
            return AddService(typeof(TInterceptorType));
        }

        public IInterceptorMetadata AddService(Type interceptorType)
        {
            if (interceptorType == null)
            {
                throw new ArgumentNullException(nameof(interceptorType));
            }

            return AddService(interceptorType, 0);
        }

        public IInterceptorMetadata AddService<TInterceptorType>(int order) where TInterceptorType : IInterceptorMetadata
        {
            return AddService(typeof(TInterceptorType), order);
        }

        public IInterceptorMetadata AddService(Type interceptorType, int order)
        {
            if (interceptorType == null)
            {
                throw new ArgumentNullException(nameof(interceptorType));
            }

            if (!typeof(IInterceptorMetadata).IsAssignableFrom(interceptorType))
            {
                var message =
                    $"FormatTypeMustDeriveFromType {interceptorType.FullName} {typeof(IInterceptorMetadata).FullName}";
                throw new ArgumentException(message, nameof(interceptorType));
            }

            var filter = new ServiceFilterAttribute(interceptorType) { Order = order };
            Add(filter);
            return filter;
        }
    }
}