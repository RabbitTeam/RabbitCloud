using Rabbit.Cloud.Discovery.Abstractions;
using System;
using System.Linq;

namespace RC.Cluster.LoadBalance
{
    public class RandomAddressSelector : AddressSelector
    {
        private readonly IDiscoveryClient _discoveryClient;

        private static readonly Random Random = new Random();

        public RandomAddressSelector(IDiscoveryClient discoveryClient)
        {
            _discoveryClient = discoveryClient;
        }

        #region Overrides of AddressSelector

        protected override Uri Select(string serviceName)
        {
            var instances = _discoveryClient.GetInstances(serviceName);
            var index = Random.Next(instances.Count);

            return instances.ElementAt(index).Uri;
        }

        #endregion Overrides of AddressSelector
    }
}