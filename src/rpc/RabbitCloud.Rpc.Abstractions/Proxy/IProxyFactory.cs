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
        /// <typeparam name="T">类型。</typeparam>
        /// <param name="invocationHandler">调用处理程序。</param>
        /// <returns>代理实例。</returns>
        T GetProxy<T>(InvocationDelegate invocationHandler);
    }
}