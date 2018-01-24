namespace Rabbit.Go
{
    public interface IRequestCacheFactory
    {
        RequestCache GetRequestCache(MethodDescriptor descriptor);
    }
}