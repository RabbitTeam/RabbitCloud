using RabbitCloud.Rpc.Abstractions;
using System.Linq;
using System.Threading.Tasks;

namespace RabbitCloud.Rpc.Cluster.Cluster
{
    public class AvailableClusterInvoker : ClusterInvoker
    {
        public AvailableClusterInvoker(IDirectory directory) : base(directory)
        {
        }

        #region Overrides of ClusterInvokerBase

        protected override Task<IResult> DoInvoke(IInvocation invocation, IInvoker[] invokers, ILoadBalance loadBalance)
        {
            var invoker = invokers.FirstOrDefault(i => i.IsAvailable);
            return invoker.Invoke(invocation);
        }

        #endregion Overrides of ClusterInvokerBase
    }
}