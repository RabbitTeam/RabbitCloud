using RabbitCloud.Abstractions;
using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Abstractions.Protocol;
using RabbitCloud.Rpc.Cluster.Abstractions;
using System;
using System.Threading.Tasks;

namespace RabbitCloud.Registry.Abstractions.Cluster
{
    public class RegistryProtocol : Protocol
    {
        private readonly IRegistry _registry;
        private readonly IProtocol _protocol;
        private readonly ICluster _cluster;

        public RegistryProtocol(IRegistry registry, IProtocol protocol, ICluster cluster)
        {
            _registry = registry;
            _protocol = protocol;
            _cluster = cluster;
        }

        #region Overrides of Protocol

        /// <summary>
        /// 创建一个导出者。
        /// </summary>
        /// <param name="provider">RPC提供程序。</param>
        /// <param name="url">导出的Url。</param>
        /// <returns>服务导出者。</returns>
        protected override async Task<IExporter> CreateExporter(ICaller provider, Url url)
        {
//            await _registry.Register(url);
            return await _protocol.Export(provider);
        }

        /// <summary>
        /// 创建一个引用者。
        /// </summary>
        /// <param name="type">类型。</param>
        /// <param name="serviceUrl">服务Url。</param>
        /// <returns>服务引用者。</returns>
        protected override Task<ICaller> CreateReferer(Type type, Url serviceUrl)
        {
            var registryDirectory = new RegistryDirectory(_registry, _protocol, type, serviceUrl);
            return Task.FromResult(_cluster.Join(registryDirectory));
        }

        #endregion Overrides of Protocol
    }
}