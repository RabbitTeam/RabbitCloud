using Rabbit.Cloud.Discovery.Abstractions;

namespace Rabbit.Cloud.Client.Abstractions.Features
{
    public interface ILoadBalanceFeature
    {
        IServiceInstanceChooser ServiceInstanceChooser { get; set; }

        IServiceInstance RequestInstance { get; set; }
    }
}