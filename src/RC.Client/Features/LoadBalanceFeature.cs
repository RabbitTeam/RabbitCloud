using Rabbit.Cloud.Client.Abstractions;
using Rabbit.Cloud.Client.Abstractions.Features;
using Rabbit.Cloud.Discovery.Abstractions;

namespace Rabbit.Cloud.Client.Features
{
    public class LoadBalanceFeature : ILoadBalanceFeature
    {
        #region Implementation of ILoadBalanceFeature

        public IServiceInstanceChooser ServiceInstanceChooser { get; set; }
        public IServiceInstance RequestInstance { get; set; }

        #endregion Implementation of ILoadBalanceFeature
    }
}