using Castle.DynamicProxy;
using RabbitCloud.Abstractions;
using System;

namespace RabbitCloud.Rpc.Abstractions.Proxy.Castle
{
    /// <summary>
    /// 基于Castle动态代理的代理工厂。
    /// </summary>
    public class CastleProxyFactory : ProxyFactory
    {
        /// <summary>
        /// 代理生成器。
        /// </summary>
        private static readonly ProxyGenerator ProxyGenerator = new ProxyGenerator();

        #region Overrides of ProxyFactory

        /// <summary>
        /// 获取一个调用者。
        /// </summary>
        /// <param name="getInstance">对象实例工厂。</param>
        /// <param name="url">调用者url。</param>
        /// <returns>调用者。</returns>
        public override IInvoker GetInvoker(Func<object> getInstance, Url url)
        {
            return new ClrProxyInvoker(url, getInstance);
        }

        /// <summary>
        /// 获取一个Invoker的代理实例。
        /// </summary>
        /// <typeparam name="T">代理类型。</typeparam>
        /// <param name="invoker">调用者。</param>
        /// <param name="types"></param>
        /// <returns>Invoker代理实例。</returns>
        protected override T GetProxy<T>(IInvoker invoker, Type[] types)
        {
            var instance = ProxyGenerator.CreateInterfaceProxyWithoutTarget(typeof(T), types, new InvokerInterceptor(invoker));
            return (T)instance;
        }

        #endregion Overrides of ProxyFactory
    }
}