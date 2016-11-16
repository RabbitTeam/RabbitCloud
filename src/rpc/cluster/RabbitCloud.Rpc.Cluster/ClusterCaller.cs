using RabbitCloud.Abstractions;
using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Abstractions.Exceptions;
using RabbitCloud.Rpc.Cluster.Abstractions;
using RabbitCloud.Rpc.Cluster.LoadBalance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RabbitCloud.Rpc.Cluster
{
    public abstract class ClusterCaller : ICaller
    {
        protected IDirectory Directory { get; set; }
        private bool _isDisposable;

        protected ClusterCaller(IDirectory directory)
        {
            Directory = directory;
        }

        #region Implementation of IDisposable

        /// <summary>
        /// 执行与释放或重置非托管资源关联的应用程序定义的任务。
        /// </summary>
        public void Dispose()
        {
            if (_isDisposable)
                return;
            _isDisposable = true;
            Directory.Dispose();
        }

        #endregion Implementation of IDisposable

        #region Implementation of INode

        /// <summary>
        /// 是否可用。
        /// </summary>
        public bool IsAvailable => Directory.IsAvailable;

        /// <summary>
        /// 节点Url。
        /// </summary>
        public Url Url => Directory.Url;

        #endregion Implementation of INode

        #region Implementation of ICaller

        /// <summary>
        /// 接口类型。
        /// </summary>
        public Type InterfaceType => Directory.InterfaceType;

        /// <summary>
        /// 调用RPC请求。
        /// </summary>
        /// <param name="request">调用请求。</param>
        /// <returns>RPC请求响应结果。</returns>
        public async Task<IResponse> Call(IRequest request)
        {
            CheckDispose();
            //暂时写死。
            ILoadBalance loadBalance = new RandomLoadBalance();
            var callers = await GetCallers(request);
            return await DoCall(request, callers, loadBalance);
        }

        #endregion Implementation of ICaller

        #region Protected Method

        /// <summary>
        /// 调用RPC请求。
        /// </summary>
        /// <param name="request">调用请求。</param>
        /// <param name="callers">调用者集合。</param>
        /// <param name="loadBalance">负载均衡器。</param>
        /// <returns>RPC请求响应结果。</returns>
        protected abstract Task<IResponse> DoCall(IRequest request, IEnumerable<ICaller> callers, ILoadBalance loadBalance);

        protected Task<ICaller> Select(ILoadBalance loadBalance, IRequest request, IEnumerable<ICaller> callers)
        {
            return loadBalance.Select(callers.Where(i => i.IsAvailable), request);
        }

        protected async Task<IEnumerable<ICaller>> GetCallers(IRequest request)
        {
            return await Directory.GetCallers(request);
        }

        protected void CheckDispose()
        {
            if (_isDisposable)
                throw new RpcException("isDispose:true");
        }

        protected void CheckCallers(IEnumerable<ICaller> callers)
        {
            if (callers == null || !callers.Any())
                throw new RpcException("callers is empty");
        }

        #endregion Protected Method
    }
}