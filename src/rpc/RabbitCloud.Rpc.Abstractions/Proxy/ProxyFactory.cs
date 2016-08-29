using RabbitCloud.Abstractions;
using System;

namespace RabbitCloud.Rpc.Abstractions.Proxy
{
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

        public abstract IInvoker GetInvoker(Func<object> getInstance, Id id);

        #endregion Implementation of IProxyFactory

        public abstract T GetProxy<T>(IInvoker invoker, Type[] types);
    }
}