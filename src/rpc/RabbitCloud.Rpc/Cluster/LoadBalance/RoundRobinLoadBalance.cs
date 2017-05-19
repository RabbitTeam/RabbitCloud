using RabbitCloud.Abstractions.Utilities;
using RabbitCloud.Rpc.Abstractions;
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

            for (var i = 0; i < callers.Length; i++)
            {
                var caller = callers[(i + index) % callers.Length];
                if (caller != null)
                    return caller;
            }

            return null;
        }

        #endregion Overrides of LoadBalance

        private int GetNextPositive()
        {
            var index = Interlocked.Increment(ref _index);
            return MathUtil.GetPositive(index);
        }
    }
}