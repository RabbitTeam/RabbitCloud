using Rabbit.Cloud.Abstractions;
using System;

namespace Rabbit.Cloud.Facade.Abstractions
{
    public interface IProxyFactory
    {
        object GetProxy(Type type, RabbitRequestDelegate rabbitRequestDelegate);
    }

    public static class ProxyFactoryExtensions
    {
        public static T GetProxy<T>(this IProxyFactory proxyFactory, RabbitRequestDelegate rabbitRequestDelegate)
        {
            return (T)proxyFactory.GetProxy(typeof(T), rabbitRequestDelegate);
        }
    }
}