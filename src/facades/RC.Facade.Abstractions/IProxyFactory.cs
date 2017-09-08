namespace Rabbit.Cloud.Facade.Abstractions
{
    public interface IProxyFactory
    {
        T GetProxy<T>();
    }
}