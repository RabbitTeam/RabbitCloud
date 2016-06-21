using System;

namespace Rabbit.Rpc.Server.Implementation.ServiceDiscovery.Implementation
{
    /// <summary>
    /// 默认的服务实例工厂。
    /// </summary>
    public class DefaultServiceInstanceFactory : IServiceInstanceFactory
    {
        #region Implementation of IServiceInstanceFactory

        /// <summary>
        /// 创建指定的服务实例。
        /// </summary>
        /// <param name="serviceType">服务类型。</param>
        /// <returns>服务实例。</returns>
        public object Create(Type serviceType)
        {
            return Activator.CreateInstance(serviceType);
        }

        #endregion Implementation of IServiceInstanceFactory
    }
}