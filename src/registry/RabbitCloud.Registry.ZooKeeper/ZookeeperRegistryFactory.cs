using RabbitCloud.Abstractions;
using RabbitCloud.Registry.Abstractions;
using System;

namespace RabbitCloud.Registry.ZooKeeper
{
    public class ZookeeperRegistryFactory : IRegistryFactory
    {
        #region Implementation of IRegistryFactory

        /// <summary>
        /// 获取一个注册中心。
        /// </summary>
        /// <param name="url">注册中心url。</param>
        /// <returns>注册中心。</returns>
        public IRegistry GetRegistry(Url url)
        {
            throw new NotImplementedException();
        }

        #endregion Implementation of IRegistryFactory
    }
}