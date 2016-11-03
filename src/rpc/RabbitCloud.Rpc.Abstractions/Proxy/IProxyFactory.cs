using RabbitCloud.Abstractions;
using System;

namespace RabbitCloud.Rpc.Abstractions.Proxy
{
    /// <summary>
    /// 一个抽象的代理工厂。
    /// </summary>
    public interface IProxyFactory
    {
        /// <summary>
        /// 获取一个Invoker的代理实例。
        /// </summary>
        /// <typeparam name="T">代理类型。</typeparam>
        /// <param name="invoker">调用者。</param>
        /// <returns>Invoker代理实例。</returns>
        T GetProxy<T>(IInvoker invoker);

        /// <summary>
        /// 获取一个调用者。
        /// </summary>
        /// <param name="getInstance">对象实例工厂。</param>
        /// <param name="url">调用者url。</param>
        /// <returns>调用者。</returns>
        IInvoker GetInvoker(Func<object> getInstance, Url url);
    }
}