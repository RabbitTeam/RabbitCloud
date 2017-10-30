using Rabbit.Cloud.Client.Features;
using System.Collections.Generic;

namespace RC.Client.LoadBalance.Features
{
    public interface ILoadBalanceFeature
    {
        ICollection<ServiceUrl> ServiceUrls { get; }
    }

    public class LoadBalanceFeature : ILoadBalanceFeature
    {
        public LoadBalanceFeature(IEnumerable<ServiceUrl> serviceUrls)
        {
            ServiceUrls = new List<ServiceUrl>(serviceUrls);
        }

        public LoadBalanceFeature()
        {
            ServiceUrls = new List<ServiceUrl>();
        }

        #region Implementation of ILoadBalanceFeature

        public ICollection<ServiceUrl> ServiceUrls { get; }

        #endregion Implementation of ILoadBalanceFeature
    }
}