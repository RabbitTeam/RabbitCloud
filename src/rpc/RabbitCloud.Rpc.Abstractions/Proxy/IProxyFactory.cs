namespace RabbitCloud.Rpc.Abstractions.Proxy
{
    public interface IProxyFactory
    {
        T GetProxy<T>(ICaller caller);
    }
}