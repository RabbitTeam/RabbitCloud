using RabbitCloud.Abstractions;
using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Abstractions.Protocol;
using RabbitCloud.Rpc.Cluster.Directory;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RabbitCloud.Registry.Abstractions.Cluster
{
    public class RegistryDirectory : Directory
    {
        private readonly IRegistry _registry;
        private readonly IProtocol _protocol;

        public RegistryDirectory(IRegistry registry, IProtocol protocol, Type serviceType, Url url) : base(url)
        {
            _registry = registry;
            _protocol = protocol;
            InterfaceType = serviceType;
        }

        #region Overrides of Directory

        /// <summary>
        /// 是否可用。
        /// </summary>
        public override bool IsAvailable { get; } = true;

        /// <summary>
        /// 服务接口类型。
        /// </summary>
        public override Type InterfaceType { get; }

        /// <summary>
        /// 根据RPC请求获取该服务所有的调用者。
        /// </summary>
        /// <param name="request">RPC请求。</param>
        /// <returns>调用者集合。</returns>
        protected override async Task<IEnumerable<ICaller>> DoGetCallers(IRequest request)
        {
            var urls = await GetUrls();
            var list = new List<ICaller>();

            foreach (var url in urls)
            {
                var referer = await _protocol.Refer(InterfaceType, url);
                list.Add(referer);
            }
            return list.ToArray();
        }

        #endregion Overrides of Directory

        #region Private Method

        private async Task<IEnumerable<Url>> GetUrls()
        {
            return await _registry.Discover(Url);
        }

        #endregion Private Method
    }
}