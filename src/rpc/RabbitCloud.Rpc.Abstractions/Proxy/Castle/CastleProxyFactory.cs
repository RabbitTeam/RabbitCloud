using Castle.DynamicProxy;

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
        /// <typeparam name="T">类型。</typeparam>
        /// <param name="invocationHandler">调用处理程序。</param>
        /// <returns>代理实例。</returns>
        public T GetProxy<T>(InvocationDelegate invocationHandler)
        {
            var instance = ProxyGenerator.CreateInterfaceProxyWithoutTarget(typeof(T), new[] { typeof(T) }, new InvokerInterceptor(invocationHandler));
            return (T)instance;
        }

        #endregion Implementation of IProxyFactory
    }
}