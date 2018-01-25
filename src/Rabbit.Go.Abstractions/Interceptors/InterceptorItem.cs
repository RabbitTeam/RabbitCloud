using System;

namespace Rabbit.Go.Interceptors
{
    public class InterceptorItem
    {
        public InterceptorItem(InterceptorDescriptor descriptor)
        {
            Descriptor = descriptor ?? throw new ArgumentNullException(nameof(descriptor));
        }

        public InterceptorDescriptor Descriptor { get; }
        public IInterceptorMetadata Interceptor { get; set; }
        public bool IsReusable { get; set; }
    }
}