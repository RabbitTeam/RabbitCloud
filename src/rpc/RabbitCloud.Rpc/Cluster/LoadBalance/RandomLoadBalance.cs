using RabbitCloud.Rpc.Abstractions;
using System;
using System.Linq;
using System.Threading;

namespace RabbitCloud.Rpc.Cluster.LoadBalance
{
    public class RandomLoadBalance : LoadBalanceBase
    {
        private static readonly ThreadLocal<Random> Random = new ThreadLocal<Random>(() => new Random());

        #region Overrides of LoadBalance

        protected override ICaller DoSelect(ICaller[] callers, IRequest request)
        {
            var random = Random.Value;

            var index = (int)(random.NextDouble() * callers.Length);
            return callers.Select((t, i) => callers[(i + index) % callers.Length]).FirstOrDefault(caller => caller.IsAvailable);
        }

        #endregion Overrides of LoadBalance
    }
}