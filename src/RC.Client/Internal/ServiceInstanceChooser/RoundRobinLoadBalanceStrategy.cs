using Rabbit.Cloud.Discovery.Abstractions;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Rabbit.Cloud.Client.ServiceInstanceChooser
{
    public class RoundRobinLoadBalanceStrategy : ServiceInstanceChooser
    {
        private readonly ConcurrentDictionary<string, short> _sequences = new ConcurrentDictionary<string, short>();

        #region Overrides of LoadBalanceStrategy<TKey,TItem>

        protected override IServiceInstance DoChoose(IReadOnlyCollection<IServiceInstance> instances)
        {
            var index = _sequences.AddOrUpdate(instances.First().ServiceId, k => 0, (k, i) =>
              {
                  if (i == short.MaxValue)
                      return 0;
                  return (short)(i + 1);
              });
            return instances.ElementAt(index % instances.Count);
        }

        #endregion Overrides of LoadBalanceStrategy<TKey,TItem>
    }
}