using Rabbit.Cloud.Discovery.Abstractions;
using System;
using System.Linq;
using System.Threading;

namespace RC.Cluster.LoadBalance
{
    public class RoundRobinAddressSelector : AddressSelector
    {
        private int _index = -1;
        private readonly IDiscoveryClient _discoveryClient;

        public RoundRobinAddressSelector(IDiscoveryClient discoveryClient)
        {
            _discoveryClient = discoveryClient;
        }

        #region Overrides of AddressSelector

        protected override Uri Select(string serviceName)
        {
            var instances = _discoveryClient.GetInstances(serviceName);
            var maxIndex = instances.Count - 1;

            var index = Interlocked.Increment(ref _index);

            if (index > maxIndex || index < 0)
                index = Interlocked.Exchange(ref index, 0);

            return instances.ElementAt(index).Uri;
        }

        #endregion Overrides of AddressSelector
    }
}