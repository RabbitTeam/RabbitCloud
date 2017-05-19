using RabbitCloud.Rpc.Abstractions;
using System;
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

            var index = (int)random.NextDouble() * callers.Length;
            for (var i = 0; i < callers.Length; i++)
            {
                var caller = callers[(i + index) % callers.Length];
                //todo:是否可用
                if (caller != null)
                    return caller;
            }

            return null;
        }

        #endregion Overrides of LoadBalance
    }
}