using Rabbit.Cloud.Cluster.Abstractions.LoadBalance;

namespace Rabbit.Cloud.Cluster.LoadBalance.Features
{
    public interface ILoadBalanceFeature
    {
        IServiceInstanceChoose ServiceInstanceChoose { get; set; }
    }

    public class LoadBalanceFeature : ILoadBalanceFeature
    {
        #region Implementation of ILoadBalanceFeature

        public IServiceInstanceChoose ServiceInstanceChoose { get; set; }

        #endregion Implementation of ILoadBalanceFeature
    }
}