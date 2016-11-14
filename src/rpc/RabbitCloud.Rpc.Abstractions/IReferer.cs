using RabbitCloud.Abstractions;

namespace RabbitCloud.Rpc.Abstractions
{
    /// <summary>
    /// 一个抽象的RPC引用。
    /// </summary>
    public interface IReferer : ICaller
    {
        /// <summary>
        /// 引用的服务Url。
        /// </summary>
        Url ServiceUrl { get; }
    }
}