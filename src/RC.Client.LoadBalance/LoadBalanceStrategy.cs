using System;
using System.Collections.Generic;
using System.Linq;

namespace RC.Client.LoadBalance
{
    public abstract class LoadBalanceStrategy<T> : ILoadBalanceStrategy<T>
    {
        protected LoadBalanceStrategy(IReadOnlyCollection<T> items)
        {
            Items = items ?? throw new ArgumentNullException(nameof(items));
        }

        #region Implementation of ILoadBalanceStrategy<out T>

        public IReadOnlyCollection<T> Items { get; }

        public T Choose()
        {
            if (Items == null || !Items.Any())
                return default(T);

            return Items.Count == 1 ? Items.ElementAt(0) : DoChoose();
        }

        #endregion Implementation of ILoadBalanceStrategy<out T>

        protected abstract T DoChoose();
    }
}