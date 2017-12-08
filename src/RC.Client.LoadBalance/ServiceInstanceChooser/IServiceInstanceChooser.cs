using Rabbit.Cloud.Discovery.Abstractions;
using System.Collections.Generic;

namespace Rabbit.Cloud.Client.LoadBalance
{
    public interface IServiceInstanceChooser
    {
        IServiceInstance Choose(string serviceId, IReadOnlyList<IServiceInstance> instances);
    }
}