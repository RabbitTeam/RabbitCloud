using System.Collections.Generic;

namespace RC.Client.LoadBalance
{
    public interface ILoadBalanceStrategy<out T>
    {
        IReadOnlyCollection<T> Items { get; }

        T Choose();
    }
}