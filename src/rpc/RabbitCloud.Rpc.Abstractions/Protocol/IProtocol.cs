using RabbitCloud.Abstractions;
using System;
using System.Threading.Tasks;

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
        /// <returns>一个导出者。</returns>
        Task<IExporter> Export(ICaller provider);

        /// <summary>
        /// 引用一个RPC服务。
        /// </summary>
        /// <param name="type">本地服务类型。</param>
        /// <param name="serviceUrl">服务Url。</param>
        /// <returns>一个引用者。</returns>
        Task<ICaller> Refer(Type type, Url serviceUrl);
    }
}