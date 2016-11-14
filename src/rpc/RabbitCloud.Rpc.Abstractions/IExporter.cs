namespace RabbitCloud.Rpc.Abstractions
{
    /// <summary>
    /// 一个抽象的RPC导出者。
    /// </summary>
    public interface IExporter : INode
    {
        /// <summary>
        /// 调用提供程序。
        /// </summary>
        IProvider Provider { get; }
    }
}