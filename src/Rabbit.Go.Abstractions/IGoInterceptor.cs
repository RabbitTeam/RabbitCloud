namespace Rabbit.Go.Abstractions
{
    public interface IGoInterceptor
    {
        void Apply(RequestContext context);
    }
}