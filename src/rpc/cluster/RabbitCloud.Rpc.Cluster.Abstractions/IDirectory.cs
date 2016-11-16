using RabbitCloud.Rpc.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RabbitCloud.Rpc.Cluster.Abstractions
{
    /// <summary>
    /// 一个抽象的服务目录。
    /// </summary>
    public interface IDirectory : INode
    {
        /// <summary>
        /// 服务接口类型。
        /// </summary>
        Type InterfaceType { get; }

        /// <summary>
        /// 根据RPC请求获取该服务所有的调用者。
        /// </summary>
        /// <param name="request">RPC请求。</param>
        /// <returns>调用者集合。</returns>
        Task<IEnumerable<ICaller>> GetCallers(IRequest request);
    }
}