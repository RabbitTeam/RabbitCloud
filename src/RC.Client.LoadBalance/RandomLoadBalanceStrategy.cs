using System;
using System.Collections.Generic;
using System.Linq;

namespace RC.Client.LoadBalance
{
    internal class RandomStatic
    {
        public static readonly Random Random = new Random();
    }

    public class RandomLoadBalanceStrategy<T> : LoadBalanceStrategy<T>
    {
        public RandomLoadBalanceStrategy(IReadOnlyCollection<T> items) : base(items)
        {
        }

        #region Overrides of LoadBalanceStrategy<T>

        protected override T DoChoose()
        {
            if (Items.Count == 1)
                return Items.ElementAt(0);

            var index = RandomStatic.Random.Next(Items.Count);
            return Items.ElementAt(index);
        }

        #endregion Overrides of LoadBalanceStrategy<T>
    }
}