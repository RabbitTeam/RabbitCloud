using Castle.DynamicProxy;
using System;

namespace RabbitCloud.Rpc.Abstractions.Proxy.Castle
{
    public class CastleProxyFactory : IProxyFactory
    {
        /// <summary>
        /// 代理生成器。
        /// </summary>
        private static readonly ProxyGenerator ProxyGenerator = new ProxyGenerator();

        #region Implementation of IProxyFactory

        /// <summary>
        /// 获取一个类型的代理。
        /// </summary>
        /// <param name="type">类型。</param>
        /// <param name="invocationHandler">调用处理程序。</param>
        /// <returns>代理实例。</returns>
        public object GetProxy(Type type, InvocationDelegate invocationHandler)
        {
            var instance = ProxyGenerator.CreateInterfaceProxyWithoutTarget(type, new[] { type }, new InvokerInterceptor(invocationHandler));
            return instance;
        }

        #endregion Implementation of IProxyFactory
    }
}