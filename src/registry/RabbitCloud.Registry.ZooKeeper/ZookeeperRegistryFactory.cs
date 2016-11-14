using RabbitCloud.Abstractions;
using RabbitCloud.Registry.Abstractions;

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
            var address = $"{url.Host}:{url.Port}";
            var timeout = 0;
            string timeoutValue;
            if (url.Parameters.TryGetValue("timeout", out timeoutValue))
            {
                int.TryParse(timeoutValue, out timeout);
            }
            if (timeout <= 0)
                timeout = 20000;

            return new ZookeeperRegistry(new org.apache.zookeeper.ZooKeeper(address, timeout, null));
        }

        #endregion Implementation of IRegistryFactory
    }
}