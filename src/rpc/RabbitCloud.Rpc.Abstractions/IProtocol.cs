using RabbitCloud.Abstractions;
using System;

namespace RabbitCloud.Rpc.Abstractions
{
    /// <summary>
    /// 一个抽象的协议。
    /// </summary>
    public interface IProtocol : IDisposable
    {
        /// <summary>
        /// 导出一个调用者。
        /// </summary>
        /// <param name="invoker">调用者。</param>
        /// <returns>导出者。</returns>
        IExporter Export(IInvoker invoker);

        /// <summary>
        /// 引用一个调用者。
        /// </summary>
        /// <param name="url">调用者Url。</param>
        /// <returns>调用者。</returns>
        IInvoker Refer(Url url);
    }
}