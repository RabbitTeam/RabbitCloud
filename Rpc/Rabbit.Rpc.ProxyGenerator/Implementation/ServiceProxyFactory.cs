using Rabbit.Rpc.Client;
using Rabbit.Rpc.Serialization;
using System;
using System.Linq;

namespace Rabbit.Rpc.ProxyGenerator.Implementation
{
    /// <summary>
    /// 默认的服务代理工厂实现。
    /// </summary>
    public class ServiceProxyFactory : IServiceProxyFactory
    {
        #region Field

        private readonly IRemoteInvokeService _remoteInvokeService;
        private readonly ISerializer _serializer;

        #endregion Field

        #region Constructor

        public ServiceProxyFactory(IRemoteInvokeService remoteInvokeService, ISerializer serializer)
        {
            _remoteInvokeService = remoteInvokeService;
            _serializer = serializer;
        }

        #endregion Constructor

        #region Implementation of IServiceProxyFactory

        /// <summary>
        /// 创建服务代理。
        /// </summary>
        /// <param name="proxyType">代理类型。</param>
        /// <returns>服务代理实例。</returns>
        public object CreateProxy(Type proxyType)
        {
            var instance = proxyType.GetConstructors().First().Invoke(new object[] { _remoteInvokeService, _serializer });
            return instance;
        }

        #endregion Implementation of IServiceProxyFactory
    }
}