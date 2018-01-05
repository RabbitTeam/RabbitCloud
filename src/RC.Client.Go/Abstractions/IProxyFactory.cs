using System;

namespace Rabbit.Cloud.Client.Go.Abstractions
{
    public interface IProxyFactory
    {
        object CreateProxy(Type interfaceType);
    }

    public static class ProxyFactoryExtensions
    {
        public static T CreateProxy<T>(this IProxyFactory proxyFactory)
        {
            return (T)proxyFactory.CreateProxy(typeof(T));
        }
    }
}