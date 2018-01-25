using System;

namespace Rabbit.Go.Interceptors
{
    public class InterceptorDescriptor
    {
        public InterceptorDescriptor(IInterceptorMetadata interceptor)
        {
            Interceptor = interceptor ?? throw new ArgumentNullException(nameof(interceptor));
            if (interceptor is IOrderedInterceptor orderedInterceptor)
                Order = orderedInterceptor.Order;
        }

        public IInterceptorMetadata Interceptor { get; }
        public int Order { get; set; }
    }
}