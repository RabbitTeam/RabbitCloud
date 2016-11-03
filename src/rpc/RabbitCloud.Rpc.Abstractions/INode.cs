using RabbitCloud.Abstractions;
using System;

namespace RabbitCloud.Rpc.Abstractions
{
    /// <summary>
    /// 一个抽象的节点。
    /// </summary>
    public interface INode : IDisposable
    {
        /// <summary>
        /// 节点Url。
        /// </summary>
        Url Url { get; }

        /// <summary>
        /// 是否可用。
        /// </summary>
        bool IsAvailable { get; }
    }
}