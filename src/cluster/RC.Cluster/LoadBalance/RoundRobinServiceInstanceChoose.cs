using Rabbit.Cloud.Discovery.Abstractions;
using System.Linq;
using System.Threading;

namespace Rabbit.Cloud.Cluster.LoadBalance
{
    public class RoundRobinServiceInstanceChoose : ServiceInstanceChoose
    {
        private int _index = -1;
        private readonly IDiscoveryClient _discoveryClient;

        public RoundRobinServiceInstanceChoose(IDiscoveryClient discoveryClient)
        {
            _discoveryClient = discoveryClient;
        }

        #region Overrides of ServiceInstanceChoose

        protected override IServiceInstance Choose(string serviceName)
        {
            var instances = _discoveryClient.GetInstances(serviceName);
            if (instances == null || !instances.Any())
                return null;

            var maxIndex = instances.Count - 1;

            var index = Interlocked.Increment(ref _index);

            if (index > maxIndex || index < 0)
                index = Interlocked.Exchange(ref index, 0);

            return instances.ElementAt(index);
        }

        #endregion Overrides of ServiceInstanceChoose
    }
}