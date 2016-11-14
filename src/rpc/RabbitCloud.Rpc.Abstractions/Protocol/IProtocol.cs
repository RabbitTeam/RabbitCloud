using RabbitCloud.Abstractions;
using System;

namespace RabbitCloud.Rpc.Abstractions.Protocol
{
    /// <summary>
    /// 一个抽象的RPC协议。
    /// </summary>
    public interface IProtocol : IDisposable
    {
        /// <summary>
        /// 导出一个RPC提供程序。
        /// </summary>
        /// <param name="provider">RPC提供程序。</param>
        /// <param name="url">导出的Url。</param>
        /// <returns>一个导出者。</returns>
        IExporter Export(IProvider provider, Url url);

        /// <summary>
        /// 引用一个RPC服务。
        /// </summary>
        /// <param name="type">本地服务类型。</param>
        /// <param name="serviceUrl">服务Url。</param>
        /// <returns>一个引用者。</returns>
        IReferer Refer(Type type, Url serviceUrl);
    }
}