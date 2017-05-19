using RabbitCloud.Abstractions.Utilities;
using RabbitCloud.Rpc.Abstractions;
using System.Linq;
using System.Threading;

namespace RabbitCloud.Rpc.Cluster.LoadBalance
{
    public class RoundRobinLoadBalance : LoadBalanceBase
    {
        private int _index;

        #region Overrides of LoadBalance

        protected override ICaller DoSelect(ICaller[] callers, IRequest request)
        {
            var index = GetNextPositive();

            return callers.Select((t, i) => callers[(i + index) % callers.Length]).FirstOrDefault(caller => caller.IsAvailable);
        }

        #endregion Overrides of LoadBalance

        #region Private Method

        private int GetNextPositive()
        {
            var index = Interlocked.Increment(ref _index);
            return MathUtil.GetPositive(index);
        }

        #endregion Private Method
    }
}