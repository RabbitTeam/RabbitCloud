using RabbitCloud.Abstractions;
using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Abstractions.Exceptions;
using RabbitCloud.Rpc.Cluster.Abstractions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RabbitCloud.Rpc.Cluster.Directory
{
    /// <summary>
    /// 调用者目录抽象类。
    /// </summary>
    public abstract class Directory : IDirectory
    {
        protected Directory(Url url) : this(url, url)
        {
        }

        protected Directory(Url url, Url consumerUrl)
        {
            if (url == null)
                throw new ArgumentNullException(nameof(url));

            Url = url;
            ConsumerUrl = consumerUrl;
        }

        /// <summary>
        /// 消费者Url。
        /// </summary>
        public Url ConsumerUrl { get; set; }

        /// <summary>
        /// 是否释放。
        /// </summary>
        public bool IsDisposable { get; private set; }

        #region Implementation of INode

        /// <summary>
        /// 是否可用。
        /// </summary>
        public abstract bool IsAvailable { get; }

        /// <summary>
        /// 节点Url。
        /// </summary>
        public Url Url { get; }

        #endregion Implementation of INode

        #region Implementation of IDirectory

        /// <summary>
        /// 服务接口类型。
        /// </summary>
        public abstract Type InterfaceType { get; }

        /// <summary>
        /// 根据RPC请求获取该服务所有的调用者。
        /// </summary>
        /// <param name="request">RPC请求。</param>
        /// <returns>调用者集合。</returns>
        public async Task<IEnumerable<ICaller>> GetCallers(IRequest request)
        {
            if (IsDisposable)
                throw new RpcException($"Directory already destroyed .url: {Url}");

            var callers = await DoGetCallers(request);
            return callers;
        }

        #endregion Implementation of IDirectory

        #region Implementation of IDisposable

        /// <summary>
        /// 执行与释放或重置非托管资源关联的应用程序定义的任务。
        /// </summary>
        public virtual void Dispose()
        {
            IsDisposable = true;
        }

        #endregion Implementation of IDisposable

        #region Protected Method

        /// <summary>
        /// 根据RPC请求获取该服务所有的调用者。
        /// </summary>
        /// <param name="request">RPC请求。</param>
        /// <returns>调用者集合。</returns>
        protected abstract Task<IEnumerable<ICaller>> DoGetCallers(IRequest request);

        #endregion Protected Method
    }
}