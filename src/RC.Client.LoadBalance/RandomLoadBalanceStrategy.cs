using System;
using System.Collections.Generic;
using System.Linq;

namespace RC.Client.LoadBalance
{
    internal class RandomStatic
    {
        public static readonly Random Random = new Random();
    }

    public class RandomLoadBalanceStrategy<TKey, TItem> : LoadBalanceStrategy<TKey, TItem>
    {
        #region Overrides of LoadBalanceStrategy<TKey,TItem>

        protected override TItem DoChoose(TKey key, IReadOnlyCollection<TItem> items)
        {
            var index = RandomStatic.Random.Next(items.Count);
            return items.ElementAt(index);
        }

        #endregion Overrides of LoadBalanceStrategy<TKey,TItem>
    }
}