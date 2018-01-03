using Rabbit.Cloud.Discovery.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rabbit.Cloud.Client.ServiceInstanceChooser
{
    internal class RandomStatic
    {
        public static readonly Random Random = new Random();
    }

    public class RandomServiceInstanceChooser : ServiceInstanceChooser
    {
        #region Overrides of LoadBalanceStrategy<TKey,TItem>

        protected override IServiceInstance DoChoose(IReadOnlyCollection<IServiceInstance> instances)
        {
            var index = RandomStatic.Random.Next(instances.Count);
            return instances.ElementAt(index);
        }

        #endregion Overrides of LoadBalanceStrategy<TKey,TItem>
    }
}