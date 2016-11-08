using RabbitCloud.Rpc.Abstractions;
using System;

namespace RabbitCloud.Rpc.Cluster
{
    /// <summary>
    /// 一个抽象的调用者目录。
    /// </summary>
    public interface IDirectory : INode
    {
        /// <summary>
        /// 接口类型。
        /// </summary>
        Type InterfaceType { get; set; }

        /// <summary>
        /// 获取所有调用者。
        /// </summary>
        /// <param name="invocation">调用信息。</param>
        /// <returns>该目录下所有的调用者。</returns>
        IInvoker[] GetInvokers(IInvocation invocation);
    }
}