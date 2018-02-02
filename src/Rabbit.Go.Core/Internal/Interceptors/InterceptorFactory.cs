using Rabbit.Go.Interceptors;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rabbit.Go.Internal
{
    public class InterceptorFactory
    {
        public static InterceptorFactoryResult GetAllInterceptors(MethodDescriptor descriptor, IServiceProvider services)
        {
            var staticFilterItems = new InterceptorItem[descriptor.InterceptorDescriptors.Count];

            var orderedInterceptors = descriptor
                .InterceptorDescriptors
                .OrderBy(i => i.Order)
                .ToList();

            for (var i = 0; i < orderedInterceptors.Count; i++)
            {
                staticFilterItems[i] = new InterceptorItem(orderedInterceptors[i]);
            }


            var interceptors = new List<IInterceptorMetadata>();
            foreach (var item in staticFilterItems)
            {
                var interceptor = CreateInterceptor(services, item);
                interceptors.Add(interceptor);
            }

            for (var i = 0; i < staticFilterItems.Length; i++)
            {
                var item = staticFilterItems[i];
                if (!item.IsReusable)
                    item.Interceptor = null;
            }

            return new InterceptorFactoryResult(staticFilterItems, interceptors.ToArray());
        }

        private static IInterceptorMetadata CreateInterceptor(IServiceProvider services, InterceptorItem interceptorItem)
        {
            if (interceptorItem.Interceptor != null)
                return interceptorItem.Interceptor;

            var interceptor = interceptorItem.Descriptor.Interceptor;

            if (interceptor is IInterceptorFactory interceptorFactory)
            {
                interceptorItem.Interceptor = interceptorFactory.CreateInstance(services);
                interceptorItem.IsReusable = interceptorFactory.IsReusable;
            }
            else
            {
                interceptorItem.Interceptor = interceptor;
                interceptorItem.IsReusable = true;
            }

            return interceptorItem.Interceptor;
        }
    }
}