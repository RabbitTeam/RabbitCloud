using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace RC.Client.LoadBalance
{
    public class RoundRobinLoadBalanceStrategy<T> : LoadBalanceStrategy<T>
    {
        private int _index = -1;

        public RoundRobinLoadBalanceStrategy(IReadOnlyCollection<T> items) : base(items)
        {
        }

        #region Overrides of LoadBalanceStrategy<T>

        protected override T DoChoose()
        {
            var maxIndex = Items.Count - 1;

            var index = Interlocked.Increment(ref _index);

            if (index > maxIndex || index < 0)
                Interlocked.Exchange(ref _index, 0);

            return Items.ElementAt(_index);
        }

        #endregion Overrides of LoadBalanceStrategy<T>
    }
}