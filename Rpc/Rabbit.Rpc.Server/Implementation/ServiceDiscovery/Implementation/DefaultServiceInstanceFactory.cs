using Rabbit.Rpc.Logging;
using System;

namespace Rabbit.Rpc.Server.Implementation.ServiceDiscovery.Implementation
{
    /// <summary>
    /// 默认的服务实例工厂。
    /// </summary>
    public class DefaultServiceInstanceFactory : IServiceInstanceFactory
    {
        private readonly ILogger<DefaultServiceInstanceFactory> _logger;

        public DefaultServiceInstanceFactory(ILogger<DefaultServiceInstanceFactory> logger)
        {
            _logger = logger;
        }

        #region Implementation of IServiceInstanceFactory

        /// <summary>
        /// 创建指定的服务实例。
        /// </summary>
        /// <param name="serviceType">服务类型。</param>
        /// <returns>服务实例。</returns>
        public object Create(Type serviceType)
        {
            try
            {
                return Activator.CreateInstance(serviceType);
            }
            catch (Exception exception)
            {
                if (_logger.IsEnabled(LogLevel.Fatal))
                    _logger.Fatal($"为类型：{serviceType}创建实例时发生了错误。", exception);
                throw;
            }
        }

        #endregion Implementation of IServiceInstanceFactory
    }
}