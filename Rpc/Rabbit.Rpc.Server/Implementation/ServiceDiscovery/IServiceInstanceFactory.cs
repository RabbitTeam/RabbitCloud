using System;

namespace Rabbit.Rpc.Server.Implementation.ServiceDiscovery
{
    /// <summary>
    /// 一个抽象的服务实例工厂。
    /// </summary>
    public interface IServiceInstanceFactory
    {
        /// <summary>
        /// 创建指定的服务实例。
        /// </summary>
        /// <param name="serviceType">服务类型。</param>
        /// <returns>服务实例。</returns>
        object Create(Type serviceType);
    }
}