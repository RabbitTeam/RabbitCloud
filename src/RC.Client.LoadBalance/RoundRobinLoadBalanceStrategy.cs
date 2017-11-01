using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Rabbit.Cloud.Client.LoadBalance
{
    public class RoundRobinLoadBalanceStrategy<TKey, TItem> : LoadBalanceStrategy<TKey, TItem>
    {
        private readonly ConcurrentDictionary<TKey, short> _sequences = new ConcurrentDictionary<TKey, short>();

        #region Overrides of LoadBalanceStrategy<TKey,TItem>

        protected override TItem DoChoose(TKey key, IReadOnlyCollection<TItem> items)
        {
            var index = _sequences.AddOrUpdate(key, k => 0, (k, i) =>
              {
                  if (i == short.MaxValue)
                      return 0;
                  return (short)(i + 1);
              });
            return items.ElementAt(index % items.Count);
        }

        #endregion Overrides of LoadBalanceStrategy<TKey,TItem>
    }
}