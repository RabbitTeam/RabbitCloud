using Rabbit.Cloud.Discovery.Abstractions;
using System;
using System.Linq;

namespace Rabbit.Cloud.Cluster.LoadBalance
{
    public class RandomServiceInstanceChoose : ServiceInstanceChoose
    {
        private readonly IDiscoveryClient _discoveryClient;

        private static readonly Random Random = new Random();

        public RandomServiceInstanceChoose(IDiscoveryClient discoveryClient)
        {
            _discoveryClient = discoveryClient;
        }

        #region Overrides of ServiceInstanceChoose

        protected override IServiceInstance Choose(string serviceName)
        {
            var instances = _discoveryClient.GetInstances(serviceName);

            if (instances == null || !instances.Any())
                return null;

            var index = Random.Next(instances.Count);
            return instances.ElementAt(index);
        }

        #endregion Overrides of ServiceInstanceChoose
    }
}