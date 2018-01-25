using Rabbit.Go.Interceptors;

namespace Rabbit.Go.Internal
{
    public struct InterceptorFactoryResult
    {
        public InterceptorFactoryResult(
            InterceptorItem[] cacheableInterceptors,
            IInterceptorMetadata[] interceptors)
        {
            CacheableInterceptors = cacheableInterceptors;
            Interceptors = interceptors;
        }

        public InterceptorItem[] CacheableInterceptors { get; }

        public IInterceptorMetadata[] Interceptors { get; }
    }
}