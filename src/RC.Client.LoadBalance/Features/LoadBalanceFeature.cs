namespace RC.Client.LoadBalance.Features
{
    public interface ILoadBalanceFeature
    {
        string Strategy { get; set; }
    }

    public class LoadBalanceFeature : ILoadBalanceFeature
    {
        #region Implementation of ILoadBalanceFeature

        public string Strategy { get; set; }

        #endregion Implementation of ILoadBalanceFeature
    }
}