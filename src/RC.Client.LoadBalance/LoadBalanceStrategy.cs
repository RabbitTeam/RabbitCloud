using System.Collections.Generic;
using System.Linq;

namespace Rabbit.Cloud.Client.LoadBalance
{
    public abstract class LoadBalanceStrategy<TKey, TItem> : ILoadBalanceStrategy<TKey, TItem>
    {
        #region Implementation of ILoadBalanceStrategy<in TKey,TItem>

        public TItem Choose(TKey key, IReadOnlyCollection<TItem> items)
        {
            if (items == null || !items.Any())
                return default(TItem);

            return items.Count == 1 ? items.ElementAt(0) : DoChoose(key, items);
        }

        #endregion Implementation of ILoadBalanceStrategy<in TKey,TItem>

        protected abstract TItem DoChoose(TKey key, IReadOnlyCollection<TItem> items);
    }
}