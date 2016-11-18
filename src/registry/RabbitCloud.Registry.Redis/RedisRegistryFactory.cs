using RabbitCloud.Abstractions;
using RabbitCloud.Registry.Abstractions;
using RabbitCloud.Rpc.Abstractions.Utils.Extensions;

namespace RabbitCloud.Registry.Redis
{
    public class RedisRegistryFactory : IRegistryFactory
    {
        #region Implementation of IRegistryFactory

        /// <summary>
        /// 获取一个注册中心。
        /// </summary>
        /// <param name="url">注册中心url。</param>
        /// <returns>注册中心。</returns>
        public IRegistry GetRegistry(Url url)
        {
            var parameters = url.GetParameters();
            return new RedisRegistry(new RedisConnectionInfo
            {
                ConnectionString = parameters["ConnectionString"],
                Database = int.Parse(parameters["database"]),
                ApplicationId = parameters["application"] ?? "rabbitcloud"
            });
        }

        #endregion Implementation of IRegistryFactory
    }
}