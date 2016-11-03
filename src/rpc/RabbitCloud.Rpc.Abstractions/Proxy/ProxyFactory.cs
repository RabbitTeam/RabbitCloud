using RabbitCloud.Abstractions;
using System;

namespace RabbitCloud.Rpc.Abstractions.Proxy
{
    /// <summary>
    /// 代理工厂抽象类。
    /// </summary>
    public abstract class ProxyFactory : IProxyFactory
    {
        #region Implementation of IProxyFactory

        /// <summary>
        /// 获取一个Invoker的代理实例。
        /// </summary>
        /// <typeparam name="T">代理类型。</typeparam>
        /// <param name="invoker">调用者。</param>
        /// <returns>Invoker代理实例。</returns>
        public T GetProxy<T>(IInvoker invoker)
        {
            return GetProxy<T>(invoker, new[] { typeof(T) });
        }

        /// <summary>
        /// 获取一个调用者。
        /// </summary>
        /// <param name="getInstance">对象实例工厂。</param>
        /// <param name="url">调用者url。</param>
        /// <returns>调用者。</returns>
        public abstract IInvoker GetInvoker(Func<object> getInstance, Url url);

        #endregion Implementation of IProxyFactory

        /// <summary>
        /// 获取一个Invoker的代理实例。
        /// </summary>
        /// <typeparam name="T">代理类型。</typeparam>
        /// <param name="invoker">调用者。</param>
        /// <param name="types"></param>
        /// <returns>Invoker代理实例。</returns>
        protected abstract T GetProxy<T>(IInvoker invoker, Type[] types);
    }
}