using System;

namespace Rabbit.Cloud.Client.Proxy
{
    public interface IProxyFactory
    {
        object CreateInterfaceProxy(Type interfaceType);

        object CreateInterfaceProxy(Type interfaceType, object interfaceTarget);
    }

    public static class ProxyFactoryExtensions
    {
        public static T CreateInterfaceProxy<T>(this IProxyFactory proxyFactory)
        {
            return (T)proxyFactory.CreateInterfaceProxy(typeof(T));
        }

        public static T CreateInterfaceProxy<T>(this IProxyFactory proxyFactory, T interfaceTarget)
        {
            return (T)proxyFactory.CreateInterfaceProxy(typeof(T), interfaceTarget);
        }
    }
}