namespace Rabbit.Cloud.Client.LoadBalance
{
    public class LoadBalanceOptions
    {
        public IServiceInstanceChooserCollection ServiceInstanceChooserCollection { get; } = new ServiceInstanceChooserCollection();
    }
}