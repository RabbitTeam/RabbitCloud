namespace Rabbit.Cloud.Client.LoadBalance.Features
{
    public class LoadBalanceStrategy
    {
        public int MaxAutoRetries { get; set; }
        public int MaxAutoRetriesNextServer { get; set; }
    }

    public interface ILoadBalanceFeature
    {
        IServiceInstanceChooser ServiceInstanceChooser { get; set; }
        LoadBalanceStrategy Strategy { get; set; }
    }

    public class LoadBalanceFeature : ILoadBalanceFeature
    {
        #region Implementation of ILoadBalanceFeature

        public IServiceInstanceChooser ServiceInstanceChooser { get; set; }
        public LoadBalanceStrategy Strategy { get; set; }

        #endregion Implementation of ILoadBalanceFeature
    }
}