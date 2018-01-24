namespace Rabbit.Go.Interceptors
{
    public interface IOrderedInterceptor
    {
        int Order { get; }
    }
}