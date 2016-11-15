using RabbitCloud.Rpc.Abstractions.Features;
using System;

namespace RabbitCloud.Rpc.Abstractions.Hosting.Server
{
    /// <summary>
    /// 一个抽象的Rcp服务器。
    /// </summary>
    public interface IRpcServer : IDisposable
    {
        /// <summary>
        /// 特性集合。
        /// </summary>
        IRpcFeatureCollection Features { get; }

        /// <summary>
        /// 启动服务器。
        /// </summary>
        /// <typeparam name="TContext">上下文类型。</typeparam>
        /// <param name="application">应用程序实例。</param>
        void Start<TContext>(IRpcApplication<TContext> application);
    }
}