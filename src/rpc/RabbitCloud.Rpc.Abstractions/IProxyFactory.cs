using RabbitCloud.Abstractions;
using System;

namespace RabbitCloud.Rpc.Abstractions
{
    public interface IProxyFactory
    {
        /// <summary>
        /// 获取一个Invoker的代理实例。
        /// </summary>
        /// <typeparam name="T">代理类型。</typeparam>
        /// <param name="invoker">调用者。</param>
        /// <returns>Invoker代理实例。</returns>
        T GetProxy<T>(IInvoker invoker);

        IInvoker GetInvoker(Func<object> getInstance, Id id);
    }
}