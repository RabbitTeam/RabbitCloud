using Rabbit.Cloud.Client.Abstractions;
using System;

namespace Rabbit.Cloud.Guise
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