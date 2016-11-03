using System;

namespace RabbitCloud.Rpc.Abstractions
{
    /// <summary>
    /// 一个抽象的导出者。
    /// </summary>
    public interface IExporter : IDisposable
    {
        /// <summary>
        /// 调用者。
        /// </summary>
        IInvoker Invoker { get; }
    }
}