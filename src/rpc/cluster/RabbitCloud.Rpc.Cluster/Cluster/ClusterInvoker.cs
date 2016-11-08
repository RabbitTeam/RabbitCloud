using RabbitCloud.Abstractions;
using RabbitCloud.Rpc.Abstractions;
using RabbitCloud.Rpc.Cluster.LoadBalance;
using System.Threading.Tasks;

namespace RabbitCloud.Rpc.Cluster.Cluster
{
    public abstract class ClusterInvoker : IInvoker
    {
        protected IDirectory Directory { get; }

        protected ClusterInvoker(IDirectory directory)
        {
            Directory = directory;
        }

        #region Implementation of IDisposable

        /// <summary>执行与释放或重置非托管资源关联的应用程序定义的任务。</summary>
        public void Dispose()
        {
            Directory.Dispose();
        }

        #endregion Implementation of IDisposable

        #region Implementation of INode

        /// <summary>
        /// 节点Url。
        /// </summary>
        public Url Url => Directory.Url;

        /// <summary>
        /// 是否可用。
        /// </summary>
        public bool IsAvailable => Directory.IsAvailable;

        /// <summary>
        /// 进行调用。
        /// </summary>
        /// <param name="invocation">调用信息。</param>
        /// <returns>返回结果。</returns>
        public Task<IResult> Invoke(IInvocation invocation)
        {
            ILoadBalance loadBalance = new RandomLoadBalance();
            var invokers = GetInvokers(invocation);
            return DoInvoke(invocation, invokers, loadBalance);
        }

        #endregion Implementation of INode

        protected IInvoker[] GetInvokers(IInvocation invocation)
        {
            return Directory.GetInvokers(invocation);
        }

        protected abstract Task<IResult> DoInvoke(IInvocation invocation, IInvoker[] invokers,
            ILoadBalance loadBalance);
    }
}