using System;
using System.Threading.Tasks;

namespace RabbitCloud.Rpc.Abstractions
{
    /// <summary>
    /// 一个抽象的RPC调用者。
    /// </summary>
    public interface ICaller : INode
    {
        /// <summary>
        /// 接口类型。
        /// </summary>
        Type InterfaceType { get; }

        /// <summary>
        /// 调用RPC请求。
        /// </summary>
        /// <param name="request">调用请求。</param>
        /// <returns>RPC请求响应结果。</returns>
        Task<IResponse> Call(IRequest request);
    }
}