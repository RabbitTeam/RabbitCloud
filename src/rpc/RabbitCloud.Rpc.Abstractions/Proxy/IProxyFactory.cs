using System;

namespace RabbitCloud.Rpc.Abstractions.Proxy
{
    /// <summary>
    /// 一个抽象的代理工厂。
    /// </summary>
    public interface IProxyFactory
    {
        /// <summary>
        /// 获取一个类型的代理。
        /// </summary>
        /// <param name="type">类型。</param>
        /// <param name="invocationHandler">调用处理程序。</param>
        /// <returns>代理实例。</returns>
        object GetProxy(Type type, InvocationDelegate invocationHandler);
    }

    public static class ProxyFactoryExtensions
    {
        /// <summary>
        /// 获取一个类型的代理。
        /// </summary>
        /// <typeparam name="T">类型。</typeparam>
        /// <param name="proxyFactory">代理工厂。</param>
        /// <param name="invocationHandler">调用处理程序。</param>
        /// <returns>代理实例。</returns>
        public static T GetProxy<T>(this IProxyFactory proxyFactory, InvocationDelegate invocationHandler)
        {
            return (T)proxyFactory.GetProxy(typeof(T), invocationHandler);
        }
    }
}