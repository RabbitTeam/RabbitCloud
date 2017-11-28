using Rabbit.Cloud.Discovery.Abstractions;
using System.Collections.Generic;

namespace Rabbit.Cloud.Client.LoadBalance.Features
{
    public interface ILoadBalanceFeature
    {
        string Strategy { get; set; }
        ILoadBalanceStrategy<string, IServiceInstance> LoadBalanceStrategy { get; set; }
        ICollection<IServiceInstance> ServiceInstances { get; }
        IServiceInstance SelectedServiceInstance { get; set; }
    }

    public class LoadBalanceFeature : ILoadBalanceFeature
    {
        #region Implementation of ILoadBalanceFeature

        public string Strategy { get; set; }
        public ILoadBalanceStrategy<string, IServiceInstance> LoadBalanceStrategy { get; set; }
        public ICollection<IServiceInstance> ServiceInstances { get; } = new List<IServiceInstance>();
        public IServiceInstance SelectedServiceInstance { get; set; }

        #endregion Implementation of ILoadBalanceFeature
    }
}