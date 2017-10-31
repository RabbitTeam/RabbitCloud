using System.Collections.Generic;

namespace RC.Client.LoadBalance
{
    public interface ILoadBalanceStrategy<in TKey, TItem>
    {
        TItem Choose(TKey key, IReadOnlyCollection<TItem> items);
    }
}